namespace GenCode
module Encoder =
    open System
    open System.Collections.Generic    
    type Codon = char * char * char 
    type Gene = Codon list * Codon                   
    type InvalidStates =
        | UnexpectedEnd of char []
        | InvalidCharacters of char []
        | NoStopCodon 
           
    let rec skip_comment () : (Char) -> bool =
        let mutable skip = false
        fun (c:Char) ->
            if c = '>' then
                skip <- true; false
            elif skip && c='\n' then
                skip <- false; false
            elif skip then
                false
            else
                true
    
    let is_valid_rna (chr: char[]) =
        Array.forall (fun c-> c = 'A' || c = 'U'|| c = 'G'|| c = 'C') chr
        
    let to_codon (arr: char[]): Result<Codon, InvalidStates> =
                            
        if Array.length arr = 3 then                               
            if is_valid_rna arr then                    
                Ok (arr.[0],arr.[1],arr.[2])
            else
                Error(InvalidCharacters(arr))
        else
            Error(UnexpectedEnd(arr))

    let (|StopCodon|_|) (result: Result<Codon, InvalidStates>) =
        match result with
        | Ok ('U', 'A', 'G') -> Some ('U', 'A', 'G')
        | Ok ('U', 'G', 'A') -> Some ('U', 'G', 'A')
        | Ok ('U', 'A', 'A') -> Some ('U', 'A', 'A')
        | _ -> None

    let (|Codon|_|) (result: Result<Codon, InvalidStates>) =
        match result with
        | Ok codon -> Some codon
        | _ -> None
    
    let genes_from_codons (codons: seq<char[]>): Result<Gene, InvalidStates> seq=                             
       seq {             
         use e = codons.GetEnumerator()
         let mutable (lst: Codon list) = List.empty
         let mutable cont = true
         while cont do
             if e.MoveNext() then
                let arr = to_codon e.Current
                match arr with                             
                    | StopCodon codon -> if not lst.IsEmpty then
                                            yield Ok (lst, codon); lst <- List.empty                                       
                    | Codon codon -> lst <- lst @ [codon]                                                  
                    | Error e -> yield Error e; cont <- false
             else 
                if not lst.IsEmpty then
                    yield Error NoStopCodon                                                        
                cont <- false
    }
               
    let tokens e =
        Seq.filter (skip_comment ()) e |> Seq.filter (fun c -> not(Char.IsWhiteSpace(c))) 
    
    let genes (s: seq<char>) : Result<Gene, InvalidStates> seq =        
        s |> tokens |> Seq.map Char.ToUpper |> Seq.chunkBySize 3 |> genes_from_codons    
                                                        
    module Test =     
        open NUnit.Framework
        open FsUnit

        [<Test>]
        let ``Retrieve tokens from sequence, ignoring whitespace and comments`` () = 
            let t = tokens "c G\na\n>BLAB ALA\nU"
                        
            let e = t.GetEnumerator()
            e.MoveNext() |> ignore            
            e.Current |> should equal 'c'
            e.MoveNext() |> ignore            
            e.Current |> should equal 'G'
            e.MoveNext() |> ignore            
            e.Current |> should equal 'a'
            e.MoveNext() |> ignore                                    
            e.Current |> should equal 'U'
            e.MoveNext() |> should equal false

        [<Test>]
        let ``Testing whether array of chars represents valid RNA codon (triplet)`` () =
             is_valid_rna [|'A';'U'; 'G'|] |> should equal true
             is_valid_rna [|'G';'C'; 'A'|] |> should equal true
             is_valid_rna [|'T';'U'; 'G'|] |> should equal false //DNA

        [<Test>]
        let ``Testing conversion of char array to Codon tuple`` () =
             match to_codon [|'A';'T'; 'G'|] with   
                | Error (InvalidCharacters chrs) -> chrs  |> should equal [|'A';'T'; 'G'|]
                | _-> failwith "Should not have reached this point"
             match to_codon [|'A';'G' |] with   
                | Error (UnexpectedEnd chrs) -> chrs  |> should equal [|'A';'G'|]
                | _-> failwith "Should not have reached this point"
             match to_codon [|'A';'G';'U' |] with   
                | Ok codon -> codon |> should equal ('A', 'G', 'U')
                | _-> failwith "Should not have reached this point"
        
        [<Test>]
        let ``Testing converting series of tokens (char triplets) to Gene`` () =
         
            let res = genes_from_codons [[|'A'; 'A'; 'C'|];[|'C'; 'A'; 'C'|];[|'U'; 'A'; 'G'|];[|'U'; 'A'; 'G'|]] |> Seq.toList            
            match res.Head with
                | Ok gene -> gene |> should equal ([('A', 'A', 'C');('C', 'A','C')], ('U', 'A', 'G'))
                | _-> failwith "Should not have reached this point"
             
            let res = genes "aac " |> Seq.toList            
            match res.Head with
                | Error e -> e |> should equal NoStopCodon 
                | _-> failwith "Should not have reached this point" 

        
        [<Test>]
        let ``Testing converting string to Genes`` () =
                     
            let res1 = genes "aac cac uag uag" |> Seq.toList            
            match res1.Head with
                | Ok gene -> gene |> should equal ([('A', 'A', 'C');('C', 'A','C')], ('U', 'A', 'G'))
                | _-> failwith "Should not have reached this point" 
            
            let res2 = genes "aaccacuagua" |> Seq.toList            
            match res2.[1] with
                | Error e -> e |> should equal (UnexpectedEnd [|'U'; 'A'|])
                | _ -> failwith "Should not have reached this point" 
                
            let str = """uuucaugug cccaaaauc cucucaggc auggucaag cccauccuu uuccacaac acagccuag
>NM_001293063 1
augugcgag gacugcugu gcugcaacu guuuuccgu ccuuucuuu cacuaa"""
                        
            let res3 = genes str |> Seq.toList            
            List.length res3 |> should equal 2
            match res3.[0] with
                | Ok (gene, stop) -> List.length gene |> should equal 20 // 21 - 1
                | _-> failwith "Should not have reached this point" 
            match res3.[1] with
                | Ok (gene, stop) -> List.length gene |> should equal 16 // 17 - 1
                | _-> failwith "Should not have reached this point" 

           