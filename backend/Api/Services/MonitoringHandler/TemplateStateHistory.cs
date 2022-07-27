using Api.Domain.Common;

namespace Api.Services.MonitoringHandler
{
    public class TemplateStateHistory
    {
        public TemplateState PreviousState { get; set; }
        public TemplateState PreviousValidState { get; set; }
    }
}