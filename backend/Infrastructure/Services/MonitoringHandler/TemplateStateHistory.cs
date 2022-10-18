using Domain.Common;

namespace Application.Services.MonitoringHandler
{
    public class TemplateStateHistory
    {
        public TemplateState? PreviousState { get; set; }
        public TemplateState? PreviousValidState { get; set; }
    }
}