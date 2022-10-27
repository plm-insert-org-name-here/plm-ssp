using System.Collections.Generic;
using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Step : BaseEntity
{
    public int? OrderNum { get; set; }
    public TemplateState ExpectedInitialState { get; set; }
    public TemplateState ExpectedSubsequentState { get; set; }

    public Object Object { get; set; } = default!;
    public int ObjectId { get; set; }
    // public List<Event> Events { get; set; } = default!;
    // public Template Template { get; set; } = default!;
    // public int TemplateId { get; set; }

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