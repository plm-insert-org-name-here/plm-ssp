namespace Api.Services.ProcessorHandler
{
    public class QAResultPacket
    {
        public QAResult Result { get; set; }
    }

    public enum QAResult
    {
        Success,
        Failure,
        Uncertain
    }
}