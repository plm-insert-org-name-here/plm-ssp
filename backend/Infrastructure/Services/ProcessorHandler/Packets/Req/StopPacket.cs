using System;

namespace Application.Services.ProcessorHandler.Packets.Req
{
    public class StopPacket : IReqPacket
    {
        public StopPacket(int taskId)
        {
            TaskId = taskId;
        }

        public ReqPacketType Type => ReqPacketType.Stop;
        private int TaskId { get; }

        public byte[] ToBytes() => BitConverter.GetBytes(TaskId);
    }
}