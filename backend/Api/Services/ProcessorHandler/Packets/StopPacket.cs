using System;

namespace Api.Services.ProcessorHandler.Packets
{
    public class StopPacket
    {
        public int DetectorId { get; set; }

        public byte[] ToBytes() => BitConverter.GetBytes(DetectorId);
    }
}