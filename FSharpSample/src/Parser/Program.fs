namespace Parser

open Plotly.NET

open App.AppUtil
open NetLib.Network

module Program =

    [<EntryPoint>]
    let main (args: array<string>) : int =
        plotCsvData(args[0],
            (fun p -> p.Timestamp),
            (fun p -> float p.Length),
            (fun (x, y) -> Chart.Line(x, y, Name = "Network Traffic")
                        |> Chart.withTitle "Network Traffic Size Over Time"
                        |> Chart.withXAxisStyle "Time"
                        |> Chart.withYAxisStyle "Message Length (bytes)"),
            "network_traffic_from_csv.html")

        0 // exit