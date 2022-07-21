using System;
using System.Collections.Generic;

namespace Api.Services.ProcessorHandler
{
    public class ResultPacket
    {
        public int DetectorId { get;set; }
        public List<TemplateState> TemplateStates { get; set; }

        // TODO(rg): implement
        public static ResultPacket FromBytes(byte[] buffer)
        {
            // result byte stream format:
            // 4 bytes: detector id
            // 4 bytes: # of template states
            // x * 5 bytes: list of template states

            // each template state:
            // 4 bytes: template id
            // 1 byte: template state
            throw new NotImplementedException();
        }

    }

    public class TemplateState
    {
        public int TemplateId { get; set; }

        // How the state should be interpreted depends on the type of the task,
        // hence the generic "byte" type
        public byte State { get; set; }
    }
}