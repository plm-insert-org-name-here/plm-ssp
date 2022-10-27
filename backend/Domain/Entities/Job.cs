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

    public Job(string name)
    {
        Name = name;
    }

    public void DeleteTask(Task task)
    {
        Tasks.Remove(task);
    }
}