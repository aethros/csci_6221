namespace CSInterop
{
    using System;
    using System.Collections.Generic;
    using SharpPcap;
    using SharpPcap.LibPcap;
    using PacketDotNet;
    using static Types.Network;

    /// <summary>
    /// Helper class for interoperating between C# and F# code.
    /// </summary>
    public static class CSHandler
    {
        /// <summary>
        /// PacketInfoCallback
        /// </summary>
        /// <param name="rawCapture">PacketCapture</param>
        /// <param name="linkType">LinkLayers</param>
        /// <returns></returns>
        public delegate Nullable<PacketInfo> PacketInfoCallback(PacketCapture rawCapture, LinkLayers linkType);

        /// <summary>
        /// Subscription function to properly marshal F# data.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="packetInfo"></param>
        /// <param name="captures"></param>
        /// <param name="callback"></param>
        public static void SubscribeToPacketArrival(LibPcapLiveDevice device, List<PacketInfo> packetInfo, int captures, PacketInfoCallback callback)
        {
            if (device is null || packetInfo is null || captures <= 0 || callback is null)
            {
                throw new ArgumentException("One or more of the supplied arguments were invalid for this operation.");
            }
            int capturedCount = 0;
            void OnPacketArrival(object _, PacketCapture e)
            {
                Console.WriteLine("OnPacketArrival() invoked");
                if (capturedCount < captures)
                {
                    var pkt = callback.Invoke(e, e.Device.LinkType);
                    if (pkt.HasValue)
                    {
                        packetInfo.Add(pkt.Value);
                        capturedCount++;
                        Console.WriteLine("List<PacketInfo>.Add() invoked");
                    }
                }
            }
            device.OnPacketArrival += OnPacketArrival;
            Console.WriteLine("OnPacketArrival() subscribed");
        }
    }
}