using System.Text.Json.Serialization;
using Domain.Serializers;

namespace Domain.Common.DetectorCommand;

[JsonConverter(typeof(DetectorCommandJsonConverter))]
public abstract class DetectorCommand
{
    public const string RestartMessage = "restart";
    public const string StartDetectionMessage = "start";
    public const string StopDetectionMessage = "stop";
    public const string ResumeDetectionMessage = "resume";
    public const string PauseDetectionMessage = "pause";

    [JsonPropertyName("msg")] public abstract string Message { get; }

    [JsonIgnore] public bool IsRestart => GetType() == typeof(Restart);
    [JsonIgnore] public bool IsStartDetection => GetType() == typeof(StartDetection);
    [JsonIgnore] public bool IsStopDetection => GetType() == typeof(StopDetection);
    [JsonIgnore] public bool IsResumeDetection => GetType() == typeof(ResumeDetection);
    [JsonIgnore] public bool IsPauseDetection => GetType() == typeof(PauseDetection);

    public class Restart : DetectorCommand
    {
        [JsonPropertyName("msg")] public override string Message => RestartMessage;
    }

    public class StartDetection : DetectorCommand
    {
        [JsonPropertyName("msg")] public override string Message => StartDetectionMessage;
        [JsonPropertyName("task_id")] public int TaskId { get; }

        public StartDetection(int taskId)
        {
            TaskId = taskId;
        }
    }

    public class StopDetection : DetectorCommand
    {
        [JsonPropertyName("msg")] public override string Message => StopDetectionMessage;
    }

    public class ResumeDetection : DetectorCommand
    {
        [JsonPropertyName("msg")] public override string Message => ResumeDetectionMessage;
    }

    public class PauseDetection : DetectorCommand
    {
        [JsonPropertyName("msg")] public override string Message => PauseDetectionMessage;
    }
}