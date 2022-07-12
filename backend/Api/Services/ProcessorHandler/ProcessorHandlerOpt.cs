namespace Api.Services.ProcessorHandler
{
    public class ProcessorHandlerOpt
    {
        public const string SectionName = "ProcessorHandler";

        public string UnixSocketPath { get; set; } = default!;
        public int ResponseBufferSize { get; set; }
        public int ReceiverPauseMilliseconds { get; set; }
    }
}