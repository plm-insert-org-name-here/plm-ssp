using System.Collections.Generic;
using Api.Domain.Common;
using Api.Domain.Entities;

namespace Api.Services.ProcessorHandler.Packets.Res
{
    using StateChangeId = System.Int32;
    public class KitResultPacket : ResultPacketBase
    {
        public List<(StateChangeId, TemplateState)> TemplateStates { get; set; } = default!;
    }
}