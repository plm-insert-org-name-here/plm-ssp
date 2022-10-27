using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Step : IBaseEntity
{
    public int Id { get; set; }
    public int? OrderNum { get; set; }
    public TemplateState ExpectedInitialState { get; set; }
    public TemplateState ExpectedSubsequentState { get; set; }

    private Step()
    {
    }

    public Object Object { get; set; } = default!;
    public int ObjectId { get; set; }

    public static Step Create(int newOrderNum, TemplateState newExpectedInitialState, TemplateState newExpectedSubsequentState, int newObjectId)
    {
        return new Step()
        {
            OrderNum = newOrderNum,
            ExpectedInitialState = newExpectedInitialState,
            ExpectedSubsequentState = newExpectedSubsequentState,
            ObjectId = newObjectId
        };
    }
}