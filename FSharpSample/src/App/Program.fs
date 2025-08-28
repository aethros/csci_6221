// For more information see https://aka.ms/fsharp-console-apps

open System
open Library.Json

[<EntryPoint>]
let main args =
    printfn "Hello from F#"

    printfn "Nice command-line arguments! Here's what System.Text.Json has to say about them:"

    let value, json = getJson {| args=args; year=System.DateTime.Now.Year |}
    printfn $"Input: %0A{value}"
    printfn $"Output: %s{json}"

    0 // return an integer exit code