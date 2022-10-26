using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Object : BaseEntity
{
    public string Name { get; set; } = default!;
    public ObjectCoordinates Coordinates { get; set; } = default!;

    public static Object Create(string newName, ObjectCoordinates newCoordinates)
    {
        return new Object()
        {
            Name = newName,
            Coordinates = newCoordinates
        };
    }
}