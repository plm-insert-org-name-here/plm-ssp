using System;

namespace Api.Services.ProcessorHandler.Packets.Req
{
    public class PausePacket : IReqPacket
    {
        public PausePacket(int taskId)
        {
            TaskId = taskId;
        }

        public ReqPacketType Type => ReqPacketType.Pause;
        private int TaskId { get; }

        public byte[] ToBytes() => BitConverter.GetBytes(TaskId);
    }
}