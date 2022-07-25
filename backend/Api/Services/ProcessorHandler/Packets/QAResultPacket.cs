using Api.Domain.Common;

namespace Api.Services.ProcessorHandler.Packets
{
    public class QAResultPacket : ResultPacketBase
    {
        public QAResult Result { get; set; }
    }
}