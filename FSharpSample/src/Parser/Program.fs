namespace Parser

open System.IO

open Plotly.NET
open SharpPcap.LibPcap

open App.AppUtil
open NetLib.Network

module Program =

    [<EntryPoint>]
    let main (args: array<string>) : int =
        plotCsvData
            (args[0],
            (fun p -> p.Timestamp),
            (fun p -> float p.Length),
            (fun x y -> Chart.Line(x, y, Name = "Network Traffic")),
            "network_traffic_from_csv.html")

        0 // exit