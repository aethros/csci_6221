namespace Types

open System

module Network =

    [<CLIMutable>]
    [<Struct>]
    // `type` typename = { membername: type; }
    type NetworkDevice =
        { Name: string
          Description: string
          IsActive: bool }

    [<CLIMutable>]
    [<Struct>]
    // `type` typename = { membername: type; }
    type PacketInfo =
        { Timestamp: DateTime
          SourceIP: string
          DestinationIP: string
          Protocol: string
          Length: int }

    // Helper to convert F# Option to C# Nullable
    let toNullableStruct (o: 'T option) : Nullable<'T> =
        match o with
        | Some v -> Nullable<'T>(v)
        | None -> Nullable<'T>()