// For more information see https://aka.ms/fsharp-console-apps

open System
open Library.Json
open Library.Say

// `type` typename = {| membername: type; |}
type serialized = {| args: array<string>; year: int |}

// Impure function
let output (arg: string, value: serialized, json: string) : unit =
    hello arg
    printfn "Hello from F#"
    printfn "Nice command-line arguments! Here's what System.Text.Json has to say about them:"
    printfn $"Input: %0A{value}"
    printfn $"Output: %s{json}"

[<EntryPoint>]
// let funcname (args: type): returnType = /* body */
let main (args: array<string>) : int =
    if args.Length < 3 then
        -1
    else
        // let var: type = value
        let arg: string = args[1]
        let args: array<string> = args[2..]

        let (value: serialized), (json: string) =
            getJson
                {| args = args
                   year = DateTime.Now.Year |}

        output (arg, value, json) // Side effects

        0 // return an integer exit code
