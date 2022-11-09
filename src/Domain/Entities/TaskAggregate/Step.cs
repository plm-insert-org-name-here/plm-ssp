using System.Text;
using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Step : IBaseEntity
{
    public int Id { get; set; }
    public int? OrderNum { get; set; }
    public TemplateState ExpectedInitialState { get; set; }
    public TemplateState ExpectedSubsequentState { get; set; }

    public Object Object { get; set; } = default!;
    public int ObjectId { get; set; }

    public int TaskId { get; set; }

    private Step()
    {
    }

    private Step(int id, int? orderNum, TemplateState init, TemplateState subs, int objectId, int taskId)
    {
        Id = id;
        OrderNum = orderNum;
        ExpectedInitialState = init;
        ExpectedSubsequentState = subs;
        ObjectId = objectId;
        TaskId = taskId;
    }

    public static Step Create(int newOrderNum, TemplateState newExpectedInitialState, TemplateState newExpectedSubsequentState, int newObjectId, int newTaskId)
    {
        return new Step
        {
            OrderNum = newOrderNum,
            ExpectedInitialState = newExpectedInitialState,
            ExpectedSubsequentState = newExpectedSubsequentState,
            ObjectId = newObjectId,
            TaskId = newTaskId
        };
    }
}