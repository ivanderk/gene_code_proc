// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp
open System

// Define a function to construct a message to print
open System.IO
open GenCode.Encoder

let read_file path : char seq = seq {
        
        for l in File.ReadLines(path) do
            yield! (l + "\n")                                               
}
    
[<EntryPoint>]
let main argv =
    if not (Array.length argv = 1) then
        printfn "USAGE: gencode <<path to file with rna sequence, ie. filename.fa.txt"
        0
    else
        let path = argv.[0]
        for res in read_file path |> genes do        
             match res with
                | Ok gene -> printfn "Gene( %A )" gene                 
                | Error e -> printfn "Gene( %A )" e 
                |> ignore
        
        printfn "Printing genes in %s finished" path        
        0    
