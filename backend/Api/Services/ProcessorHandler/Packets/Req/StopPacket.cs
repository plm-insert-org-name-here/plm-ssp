using System;

namespace Api.Services.ProcessorHandler.Packets.Req
{
    public class StopPacket : IReqPacket
    {
        public PacketType Type => PacketType.Stop;
        public int DetectorId { get; set; }

        public byte[] ToBytes() => BitConverter.GetBytes(DetectorId);
    }
}