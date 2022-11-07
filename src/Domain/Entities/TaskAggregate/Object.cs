using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public partial class Object : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ObjectCoordinates Coordinates { get; set; } = default!;
    public int TaskId { get; set; }

    private Object()
    {
    }

    private Object(int id, string name, ObjectCoordinates coordinates, int taskId)
    {
        Id = id;
        Name = name;
        Coordinates = coordinates;
        TaskId = taskId;
    }

    public static Object Create(string newName, ObjectCoordinates newCoordinates)
    {
        return new Object
        {
            Name = newName,
            Coordinates = newCoordinates
        };
    }
}