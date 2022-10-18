using System;

namespace Application.Services.ProcessorHandler.Packets.Req
{
    public class FramePacket : IReqPacket
    {
        public ReqPacketType Type => ReqPacketType.Frame;

        public FramePacket(int detectorId, int frameSize, byte[] frame)
        {
            DetectorId = detectorId;
            FrameSize = frameSize;
            Frame = frame;
        }

        private int DetectorId { get; }
        private int FrameSize { get; }
        private byte[] Frame { get; }


        public byte[] ToBytes()
        {
            var bytes = new byte[4 + 4 + FrameSize];

            var detectorIdBytes = BitConverter.GetBytes(DetectorId);

            var frameSizeBytes = BitConverter.GetBytes(FrameSize);

            Buffer.BlockCopy(detectorIdBytes, 0, bytes, 0, 4);
            Buffer.BlockCopy(frameSizeBytes, 0, bytes, 4, 4);
            Buffer.BlockCopy(Frame, 0, bytes, 8, FrameSize);

            return bytes;
        }
    }
}