using System.Collections.Generic;
using Domain.Common;

namespace Application.Services.ProcessorHandler.Packets.Res
{
    using StateChangeId = System.Int32;
    public class KitResultPacket : ResultPacketBase
    {
        public List<(StateChangeId, TemplateState)> TemplateStates { get; set; } = default!;
    }
}