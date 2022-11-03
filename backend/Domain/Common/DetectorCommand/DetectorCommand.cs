using System.Text.Json.Serialization;

namespace Domain.Common.DetectorCommand;

public abstract class DetectorCommand
{
    public const string RestartMessage = "restart";
    public const string StartDetectionMessage = "start";
    public const string StopDetectionMessage = "stop";
    public const string ResumeDetectionMessage = "resume";
    public const string PauseDetectionMessage = "pause";
    
    [JsonPropertyName("msg")]
    public abstract string Message { get; }

    public bool IsRestart => GetType() == typeof(Restart);
    public bool IsStartDetection => GetType() == typeof(StartDetection);
    public bool IsStopDetection => GetType() == typeof(StopDetection);
    public bool IsResumeDetection => GetType() == typeof(ResumeDetection);
    public bool IsPauseDetection => GetType() == typeof(PauseDetection);

    public class Restart : DetectorCommand
    {
        public override string Message => RestartMessage;
    }

    public class StartDetection : DetectorCommand
    {
        public override string Message => StartDetectionMessage;
        [JsonPropertyName("task_id")] public int TaskId { get; }

        public StartDetection(int taskId)
        {
            TaskId = taskId;
        }
}
    
    public class StopDetection : DetectorCommand
    {
        public override string Message => StopDetectionMessage;
    }
    
    public class ResumeDetection : DetectorCommand
    {
        public override string Message => ResumeDetectionMessage;
    }
    
    public class PauseDetection : DetectorCommand
    {
        public override string Message => PauseDetectionMessage;
    }
}