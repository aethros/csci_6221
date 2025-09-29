namespace NetLib

open SharpPcap
open SharpPcap.LibPcap
open PacketDotNet
open Types.Network

module Network =

    let getNetworkDevices (devices: LibPcapLiveDeviceList) : list<NetworkDevice> =
        devices
        |> Seq.map (fun (device: LibPcapLiveDevice) ->
            { Name = device.Name
              Description = device.Description
              IsActive = device.Opened })
        |> Seq.toList

    let parsePacketData (rawCapture: PacketCapture, linkType: LinkLayers) : option<PacketInfo> =
        try
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
        finally
            ignore None
        

    let rec getPackets (device: LibPcapLiveDevice, count: int) : list<PacketInfo> =
        if count <= 0 then
            []
        else
            let mutable capture = PacketCapture()
            let _ = device.GetNextPacket &capture
            let info = parsePacketData(capture, device.LinkType)
            match info with
            | Some value ->
                value :: getPackets(device, count - 1)
            | None ->
                getPackets(device, count - 1)

    let capturePackets (deviceName: string, packetCount: int) : list<PacketInfo> =
        let device: LibPcapLiveDevice =
            LibPcapLiveDeviceList.Instance
            |> Seq.tryFind (fun d -> d.Name = deviceName)
            |> Option.defaultWith (fun () -> failwith $"Device {deviceName} not found")
        try
            device.Open(DeviceModes.Promiscuous, 1000)
            getPackets(device, packetCount) |> Seq.toList
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

    let inline packetInfoCallback (rawCapture: PacketCapture) (linkType: LinkLayers) : System.Nullable<PacketInfo> =
        parsePacketData(rawCapture, linkType) |> toNullableStruct