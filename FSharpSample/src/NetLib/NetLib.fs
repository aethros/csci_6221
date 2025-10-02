namespace NetLib

open System
open SharpPcap
open SharpPcap.LibPcap
open PacketDotNet

module Network =

    type PacketInfo = {
        Timestamp: DateTime
        SourceIP: string
        DestinationIP: string
        Protocol: string
        DestinationPort: uint16
        Length: int }

    type NetworkDevice = {
        Name: string
        Description: string
        IsActive: bool }

    let getNetworkDevices (devices: LibPcapLiveDeviceList) : list<NetworkDevice> =
        devices
        |> Seq.map (fun (device: LibPcapLiveDevice) -> {
            Name = device.Name
            Description = device.Description
            IsActive = device.Opened })
        |> Seq.toList

    let parsePacketData (rawCapture: PacketCapture, linkType: LinkLayers) : option<PacketInfo> =
        try
            let packet: Packet = Packet.ParsePacket(linkType, rawCapture.Data.ToArray())
            let ipPacket: IPPacket = packet.Extract<IPPacket>()
            let port =
                match ipPacket.Protocol with
                | ProtocolType.Tcp -> ipPacket.Extract<TcpPacket>().DestinationPort
                | ProtocolType.Udp -> ipPacket.Extract<UdpPacket>().DestinationPort
                | _ -> uint16 -1
            match ipPacket with
            | null -> None
            | _ -> Some {
                Timestamp = rawCapture.Header.Timeval.Date
                SourceIP = ipPacket.SourceAddress.ToString()
                DestinationIP = ipPacket.DestinationAddress.ToString()
                Protocol = ipPacket.Protocol.ToString()
                DestinationPort = port
                Length = rawCapture.Data.Length }
        with
        | :? IndexOutOfRangeException -> None
        | _ -> None

    let getNPackets (device: LibPcapLiveDevice, count: int) : list<PacketInfo> =
        printfn "Capturing packets ..."
        printf "Packet %8d captured ... " 0

        let rec loop (acc: list<PacketInfo>, remaining: int): list<PacketInfo> =
            if remaining <= 0 then acc
            else
                let mutable capture = PacketCapture()
                let _ = device.GetNextPacket &capture
                match parsePacketData (capture, device.LinkType) with
                | Some (packet: PacketInfo) ->
                    printf "\rPacket %8d captured ... " acc.Length
                    loop (packet :: acc, remaining - 1)
                | None -> loop (acc, remaining)
        loop ([], count)

    let capturePackets (deviceName: string, packetCount: int) : list<PacketInfo> =
        let device: LibPcapLiveDevice =
            LibPcapLiveDeviceList.Instance
            |> Seq.tryFind (fun (d: LibPcapLiveDevice) -> d.Name = deviceName)
            |> Option.defaultWith (fun () -> failwith $"Device {deviceName} not found")
        try
            device.Open(DeviceModes.Promiscuous, 1000)
            getNPackets (device, packetCount)
        finally
            device.StopCapture()
            device.Close()

    let convertToCsv (packets: list<PacketInfo>) : list<string> =
        let header: string =
            "Timestamp,SourceIP,DestinationIP,Protocol,DestinationPort,Length"
        let csvLines: list<string> =
            packets
            |> List.map (fun (p: PacketInfo) ->
                sprintf "%s,%s,%s,%s,%u,%d"
                    (p.Timestamp.ToString "yyyy-MM-dd HH:mm:ss")
                    p.SourceIP
                    p.DestinationIP
                    p.Protocol
                    p.DestinationPort
                    p.Length)
        header :: csvLines
