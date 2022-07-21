using System.Collections.Generic;
using Api.Domain.Entities;

namespace Api.Services.ProcessorHandler
{
    public class KitResultPacket : ResultPacketBase
    {
        public List<(int, TemplateState)> TemplateStates { get; set; } = default!;
    }
}