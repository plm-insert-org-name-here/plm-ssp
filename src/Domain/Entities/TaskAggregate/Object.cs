using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Object : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ObjectCoordinates Coordinates { get; set; } = default!;

    private Object()
    {
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