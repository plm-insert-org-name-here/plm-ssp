using Microsoft.EntityFrameworkCore;

namespace Domain.Common;

[Owned]
public class EventResult
{
    public bool Success { get; set; }
    public string FailureReason { get; set; } = default!;
}