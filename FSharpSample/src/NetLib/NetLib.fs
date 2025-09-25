namespace NetLib

open System.Collections.Generic
open SharpPcap
open SharpPcap.LibPcap
open PacketDotNet
open CSInterop
open Types.Network

module Network =

    let getNetworkDevices (devices: LibPcapLiveDeviceList) : list<NetworkDevice> =
        devices
        |> Seq.map (fun (device: LibPcapLiveDevice) ->
            { Name = device.Name
              Description = device.Description
              IsActive = device.Opened })
        |> Seq.toList

    let parsePacketData (rawCapture: PacketCapture) (linkType: LinkLayers) : option<PacketInfo> =
        let packet: Packet = Packet.ParsePacket(linkType, rawCapture.Data.ToArray())
        let ipPacket: IPPacket = packet.Extract<IPPacket>()
        match ipPacket with
        | null -> None
        | _ -> Some {
            Timestamp = rawCapture.Header.Timeval.Date
            SourceIP = ipPacket.SourceAddress.ToString()
            DestinationIP = ipPacket.DestinationAddress.ToString()
            Protocol = ipPacket.Protocol.ToString()
            Length = rawCapture.Data.Length }

    let inline packetInfoCallback (rawCapture: PacketCapture) (linkType: LinkLayers) : System.Nullable<PacketInfo> =
        parsePacketData rawCapture linkType |> toNullableStruct

    let capturePackets (deviceName: string, packetCount: int) : list<PacketInfo> =
        let device: LibPcapLiveDevice =
            LibPcapLiveDeviceList.Instance
            |> Seq.tryFind (fun d -> d.Name = deviceName)
            |> Option.defaultWith (fun () -> failwith $"Device {deviceName} not found")
        try
            let packetList = List<PacketInfo>()
            CSHandler.SubscribeToPacketArrival(
                device,
                packetList,
                packetCount,
                packetInfoCallback)
            device.Open(DeviceConfiguration())
            device.StartCapture()
            packetList |> Seq.toList
        finally
            device.StopCapture()
            device.Close()

    let convertToCsv (packets: list<PacketInfo>) : list<string> =
        let header: string = "Timestamp,SourceIP,DestinationIP,Protocol,Length"
        let csvLines =
            packets
            |> List.map (fun (p: PacketInfo) ->
                sprintf
                    "%s,%s,%s,%s,%d"
                    (p.Timestamp.ToString "yyyy-MM-dd HH:mm:ss")
                    p.SourceIP
                    p.DestinationIP
                    p.Protocol
                    p.Length)
        header :: csvLines
