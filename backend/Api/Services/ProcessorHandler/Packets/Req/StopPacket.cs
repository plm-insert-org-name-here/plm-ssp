using System;

namespace Api.Services.ProcessorHandler.Packets.Req
{
    public class StopPacket : IReqPacket
    {
        public ReqPacketType Type => ReqPacketType.Stop;
        public int DetectorId { get; set; }

        public byte[] ToBytes() => BitConverter.GetBytes(DetectorId);
    }
}