using Api.Domain.Common;

namespace Api.Services.ProcessorHandler.Packets.Res
{
    public abstract class ResultPacketBase
    {
        public int DetectorId { get;set; }
        public int TaskId { get; set; }
        public JobType JobType { get; set; }
    }
}