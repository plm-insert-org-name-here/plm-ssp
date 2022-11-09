using FluentResults;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Entities;

public class Job : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Task> Tasks { get; set; } = default!;

    private Job()
    {
    }

    private Job(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public Job(string name)
    {
        Name = name;
    }

    public void DeleteTask(Task task)
    {
        Tasks.Remove(task);
    }
}