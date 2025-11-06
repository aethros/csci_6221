namespace App

open System
open System.IO
open System.Runtime.InteropServices
open System.Security.Principal

open Plotly.NET

open NetLib.Network

module AppUtil =
    [<DllImport("libc", EntryPoint = "geteuid")>]
    extern uint32 geteuid()

    let isAdministrator () =
        try
            (WindowsPrincipal
                (WindowsIdentity.GetCurrent()))
                    .IsInRole WindowsBuiltInRole.Administrator
        with e ->
            printfn "An error occurred while checking admin rights: %s" e.Message
            false

    let isElevated () =
        geteuid () = 0u

    let isWindows () =
        Environment.OSVersion
            .Platform.ToString().StartsWith "Win"

    let loadFromCsv(path: string) : array<PacketInfo> =
        printfn "Loading data from CSV: %s" path

        let lines = File.ReadAllLines path
        lines
        |> Array.skip 1 // Skip header
        |> Array.map (fun line ->
            let parts = line.Split ','
            {
                Timestamp = DateTime.Parse parts.[0]
                SourceIP = parts.[1]
                DestinationIP = parts.[2]
                Protocol = parts.[3]
                DestinationPort = uint16 parts.[4]
                Length = int parts.[5]
            })

    let plotPacketData<'T1, 'T2>
        (packets: array<PacketInfo>,
        xTransform: PacketInfo -> 'T1,
        yTransform: PacketInfo -> 'T2,
        configureChart: seq<'T1> * seq<'T2> -> GenericChart,
        filePath: string)
        : unit =
            let xValues = packets |> Array.map xTransform
            let yValues = packets |> Array.map yTransform
            let chart = configureChart(xValues, yValues)
            chart |> Chart.saveHtml filePath

    let plotCsvData<'T1, 'T2>
        (path: string,
        xTransform: PacketInfo -> 'T1,
        yTransform: PacketInfo -> 'T2,
        configureChart: seq<'T1> * seq<'T2> -> GenericChart,
        filePath: string)
        : unit =
            let packets = loadFromCsv path
            plotPacketData(packets,
                xTransform,
                yTransform, 
                configureChart,
                filePath)
