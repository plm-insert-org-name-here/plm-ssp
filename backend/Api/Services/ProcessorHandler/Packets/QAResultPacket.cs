namespace Api.Services.ProcessorHandler.Packets
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