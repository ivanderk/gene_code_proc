// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp
open System

// Define a function to construct a message to print
open System.IO
open GenCode.Encoder
open System.Text
open System.IO
open System.Net


let read_url (url:string) : char seq = seq {
              
    let req = HttpWebRequest.Create(url) :?> HttpWebRequest    
    req.Method <- "GET"
              
    let resp = req.GetResponse()
    let stream = resp.GetResponseStream()
    let reader = new StreamReader(stream)
    while not (reader.EndOfStream) do
        yield! reader.ReadLine() + "\n"                                  
}
    
let read_file path : char seq = seq {
        
    for l in File.ReadLines(path) do
        yield! (l + "\n")                                               
}
    
[<EntryPoint>]
let main argv =
    if not (Array.length argv = 1) then
        printfn "USAGE: gencode <<file path or HTTP url to file with rna sequence, ie. filename.fa.txt"
        0
    else
        let url = argv.[0]
        let read = if url.Contains("http://") || url.Contains("https://") then
                        read_url url    
                    else
                        read_file url
            
        for res in read |> genes do        
             match res with
                | Ok gene -> printfn "Gene( %A )" gene                 
                | Error e -> printfn "Gene( %A )" e 
                |> ignore
        
        printfn "Printing genes in %s finished" url        
        0    
