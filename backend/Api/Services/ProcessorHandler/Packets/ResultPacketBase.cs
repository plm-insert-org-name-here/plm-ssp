using System;
using System.Collections.Generic;
using Api.Domain.Entities;

namespace Api.Services.ProcessorHandler
{
    public abstract class ResultPacketBase
    {
        public int DetectorId { get;set; }
        public JobType JobType { get; set; }

        // TODO(rg): implement
        public static ResultPacketBase FromBytes(byte[] buffer)
        {
            // result byte stream format depends on the job type:
            // 4 bytes: detector id
            // 1 byte: job type

            // if job type is QA:
            // 1 byte: state

            // otherwise:
            // 4 bytes: # of template states
            // x * 5 bytes: list of template states
            // each template state:
            // 4 bytes: template id
            // 1 byte: template state

            // return appropriate type
            throw new NotImplementedException();
        }
    }

}