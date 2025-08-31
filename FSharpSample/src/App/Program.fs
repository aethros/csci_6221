// For more information see https://aka.ms/fsharp-console-apps

open System
open Library.Json
open Library.Say

[<EntryPoint>]

// let funcname (args: type): returnType = /* body */
let main (args: array<string>): int =
    if args.Length < 3 then
        -1

    else 
        hello args[1]
        // let var: type = value
        let args: array<string> = args[2..];
        printfn "Hello from F#"

        printfn "Nice command-line arguments! Here's what System.Text.Json has to say about them:"

        let (value: {| args: array<string>; year: int |}), (json: string) = getJson {| args=args; year=DateTime.Now.Year |}
        printfn $"Input: %0A{value}"
        printfn $"Output: %s{json}"

        0 // return an integer exit code