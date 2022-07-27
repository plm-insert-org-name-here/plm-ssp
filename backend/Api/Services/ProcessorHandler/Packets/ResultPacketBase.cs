using System;
using Api.Domain.Common;
using Api.Domain.Entities;

namespace Api.Services.ProcessorHandler.Packets
{
    public abstract class ResultPacketBase
    {
        public int DetectorId { get;set; }
        public int TaskId { get; set; }
        public JobType JobType { get; set; }
    }
}