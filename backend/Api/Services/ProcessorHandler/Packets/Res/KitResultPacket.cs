using System.Collections.Generic;
using Api.Domain.Common;

namespace Api.Services.ProcessorHandler.Packets.Res
{
    public class KitResultPacket : ResultPacketBase
    {
        public List<(int, TemplateState)> TemplateStates { get; set; } = default!;
    }
}