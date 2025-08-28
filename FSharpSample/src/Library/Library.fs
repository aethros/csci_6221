namespace Library

open System.Text.Json

module Say =
    let hello name =
        printfn "Hello %s" name

module Json =
    let getJson value =
        let json = JsonSerializer.Serialize(value)
        value, json