namespace GenCode
module Encoder =
    open System
    open System.Collections.Generic    
    type Codon = | DataCodon of char * char * char | StopCodon of char * char * char 
    type Gene = Codon list * Codon                   
    type InvalidStates =
        | UnexpectedEnd of char []
        | InvalidCharacters of char []
        | NoStopCodon 
           
    let skip_comment () : (Char) -> bool =
        let mutable skip = false
        fun c ->
            match c with
            | '>' -> skip <- true; false
            | '\n' when skip -> skip <- false; false  
            | _ -> not skip 
    
    let (|Valid|_|) (chr: char[]) =        
        match chr with                 
        | [|'U'; 'A'; 'G' |] -> Some(StopCodon('U', 'A', 'G')) 
        | [|'U'; 'G'; 'A'|] -> Some(StopCodon('U', 'G', 'A'))
        | [|'U'; 'A'; 'A'|] -> Some(StopCodon('U', 'A', 'A'))
        | [|a; b; c |] when Array.forall (fun c-> c = 'A' || c = 'U'|| c = 'G'|| c = 'C') chr -> Some(DataCodon(a,b,c))
        | _ -> None 
    
    let (|Invalid|) (chr: char[]) =
        if Array.length chr = 3 then InvalidCharacters(chr) else UnexpectedEnd(chr)
                
    let to_codon (arr: char[]): Result<Codon, InvalidStates> =
        match arr with
        | Valid codon -> Ok codon        
        | Invalid e -> Error e
           
    let genes_from_codons (codons: seq<char[]>): Result<Gene, InvalidStates> seq=                             
       seq {             
         use e = codons.GetEnumerator()
         let mutable (lst: Codon list) = List.empty
         let mutable cont = true
         while cont do
             if e.MoveNext() then
                let arr = to_codon e.Current
                match arr with                             
                    | Ok (StopCodon (a,b,c) as codon) -> if not lst.IsEmpty then
                                                            yield Ok (lst, codon); lst <- List.empty                                       
                    | Ok (DataCodon (a,b,c) as codon) -> lst <- lst @ [codon]                                                  
                    | Error e -> yield Error e; cont <- false
             else 
                if not lst.IsEmpty then
                    yield Error NoStopCodon                                                        
                cont <- false
    }
               
    let tokens e =
        Seq.filter (skip_comment ()) e |> Seq.filter (fun c -> not(Char.IsWhiteSpace(c))) |> Seq.map Char.ToUpper  
    
    let genes (s: seq<char>) : Result<Gene, InvalidStates> seq =        
        s |> tokens |> Seq.chunkBySize 3 |> genes_from_codons
        
    module Test =     
        open NUnit.Framework
        open FsUnit

        [<Test>]
        let ``Retrieve tokens from sequence, ignoring whitespace and comments`` () = 
            let t = tokens "c G\na\n>BLAB ALA\nU"
                        
            let e = t.GetEnumerator()
            e.MoveNext() |> ignore            
            e.Current |> should equal 'C'
            e.MoveNext() |> ignore            
            e.Current |> should equal 'G'
            e.MoveNext() |> ignore            
            e.Current |> should equal 'A'
            e.MoveNext() |> ignore                                    
            e.Current |> should equal 'U'
            e.MoveNext() |> should equal false
      
        [<Test>]
        let ``Testing conversion of char array to Codon tuple`` () =
             match to_codon [|'A';'T'; 'G'|] with   
                | Error (InvalidCharacters chrs) -> chrs  |> should equal [|'A';'T'; 'G'|] // DNA
                | _-> failwith "Should not have reached this point"
             match to_codon [|'A';'G' |] with   
                | Error (UnexpectedEnd chrs) -> chrs  |> should equal [|'A';'G'|]
                | _-> failwith "Should not have reached this point"
             match to_codon [|'A';'G';'U' |] with   
                | Ok (DataCodon (a, b,c) as codon) -> codon |> should equal (DataCodon ('A', 'G', 'U'))
                | _-> failwith "Should not have reached this point"
             match to_codon [|'U';'A';'A' |] with   
                | Ok (StopCodon (a, b,c) as codon) -> codon |> should equal (StopCodon ('U', 'A', 'A'))
                | _-> failwith "Should not have reached this point"
        
        [<Test>]
        let ``Testing converting series of tokens (char triplets) to Gene`` () =
         
            let res = genes_from_codons [[|'A'; 'A'; 'C'|];[|'C'; 'A'; 'C'|];[|'U'; 'A'; 'G'|];[|'U'; 'A'; 'G'|]] |> Seq.toList            
            match res.Head with
                | Ok gene -> gene |> should equal ([DataCodon('A', 'A', 'C');DataCodon('C', 'A','C')], StopCodon('U', 'A', 'G'))
                | _-> failwith "Should not have reached this point"
             
            let res = genes "aac " |> Seq.toList            
            match res.Head with
                | Error e -> e |> should equal NoStopCodon 
                | _-> failwith "Should not have reached this point" 

        
        [<Test>]
        let ``Testing converting string to Genes`` () =
                     
            let res1 = genes "aac cac uag uag" |> Seq.toList            
            match res1.Head with
                | Ok gene -> gene |> should equal ([DataCodon('A', 'A', 'C');DataCodon('C', 'A','C')], StopCodon('U', 'A', 'G'))
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

           