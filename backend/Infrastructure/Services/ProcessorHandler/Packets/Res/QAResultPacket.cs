using Domain.Common;

namespace Application.Services.ProcessorHandler.Packets.Res
{
    public class QAResultPacket : ResultPacketBase
    {
        public QAResult Result { get; set; }
    }
}