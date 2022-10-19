using System.Collections.Generic;

namespace Domain.Entities.TaskAggregate;

public class Template : BaseEntity
{
    public string Name { get; set; } = default!;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Task Task { get; set; } = default!;
    public int TaskId { get; set; }
    public List<Step> StateChanges { get; set; } = default!;
}