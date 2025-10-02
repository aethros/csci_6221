namespace App

// For more information see https://aka.ms/fsharp-console-apps
open SharpPcap.LibPcap
open System.IO
open NetLib.Network
open App.AppUtil

module Program =

    [<EntryPoint>]
    // let funcname (args: type): returnType = /* body */
    let main (args: array<string>) : int =
        let elevated: bool =
            if isWindows () then
                isAdministrator ()
            else
                isElevated ()

        if args.Length < 1 || not elevated then
            printfn "Program cannot run without administrative rights, and requires a defined capture count."
            printfn "usage: dotnet run -- <count>"
            -1
        else
        printfn "Getting network devices ..."

        let devices: list<NetworkDevice> = getNetworkDevices LibPcapLiveDeviceList.Instance
        let captureCount: int = int args[0]
        devices
        |> List.iteri (fun (i: int) (device: NetworkDevice) ->
            printfn "%2d. %4s \t-\t %s" i device.Name device.Description)

        if devices.IsEmpty then
            printfn "No devices found."
            -1
        else 
        printf "Select device (0-%d): " (devices.Length - 1)

        let deviceIndex = System.Console.ReadLine() |> int
        let packets: list<PacketInfo> =
            capturePackets (devices.Item(deviceIndex).Name, captureCount)

        if packets.IsEmpty then
            printfn "No packets captured."
            -1
        else
        printfn "\nExporting traffic ..."

        let path = "network_traffic.csv"
        let csv = convertToCsv packets
        
        printfn "Exported %d packets to %s" packets.Length path
        File.WriteAllLines(path, csv)
        0
