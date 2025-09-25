// For more information see https://aka.ms/fsharp-console-apps
open NetLib.Network
open SharpPcap.LibPcap
open System.IO
open Types.Network

[<EntryPoint>]
// let funcname (args: type): returnType = /* body */
let main (args: array<string>) : int =
    if args.Length < 1 then
        printf "No capture count specified."
        -1
    else
        printfn "Getting Network Devices ..."

        let devices: list<NetworkDevice> = getNetworkDevices LibPcapLiveDeviceList.Instance
        let captureCount: int = int args[0]
        devices
        |> List.iteri (fun (i: int) (device: NetworkDevice) -> printfn "%d. %s - %s" i device.Name device.Description)

        if not devices.IsEmpty then
            printf "Select device (0-%d): " (devices.Length - 1)

            let deviceIndex = System.Console.ReadLine() |> int
            let packets: list<PacketInfo> =
                capturePackets (devices.Item(deviceIndex).Name, captureCount)

            printfn "Capturing Packets ..."
            if not packets.IsEmpty then
                printfn "Exporting traffic..."

                let path = "network_traffic.csv"
                let csv = convertToCsv packets

                printfn "Exported %d packets to %s" packets.Length path
                File.WriteAllLines(path, csv)
                0
            else
                printfn "No packets captured."
                -1
        else
            printfn "No devices found."
            -1
