using System;

namespace Api.Services.ProcessorHandler.Packets.Req
{
    public class PausePacket : IReqPacket
    {
        public PausePacket(int detectorId)
        {
            DetectorId = detectorId;
        }

        public ReqPacketType Type => ReqPacketType.Pause;
        private int DetectorId { get; }

        public byte[] ToBytes() => BitConverter.GetBytes(DetectorId);
    }
}