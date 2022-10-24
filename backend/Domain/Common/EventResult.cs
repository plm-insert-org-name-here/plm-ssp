namespace Domain.Common;

public class EventResult
{
    public bool Failure { get; set; }
    public string FailureReason { get; set; } = default!;
}