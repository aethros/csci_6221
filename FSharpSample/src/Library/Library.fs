namespace Library

open System.Text.Json

module Say =
    let hello (name: string): unit =
        printfn "Hello %s" name

module Json =
    let getJson<'T> (value: 'T): 'T * string =
        let json: string = JsonSerializer.Serialize value
        value, json