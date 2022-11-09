using System.Net;
using Domain.Common;
using Domain.Entities.TaskAggregate;
using FastEndpoints;
using Xunit;
using Xunit.Priority;
using Endpoint = Api.Endpoints.Tasks.Update;
using Object = System.Object;
using Task = System.Threading.Tasks.Task;

namespace ApiIntegrationTests.Tasks;

[Collection("Sequential")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class Update : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public Update(Setup setup)
    {
        _client = setup.Client;
    }
    
    [Fact, Priority(0)]
    public async Task CanUpdate()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewObjects = new List<Endpoint.Req.NewObjectReq>(),
            NewSteps = new List<Endpoint.Req.NewStepReq>(),
            ModifiedObjects = new List<Endpoint.Req.ModObjectReq>(),
            ModifiedSteps = new List<Endpoint.Req.ModStepReq>(),
            DeletedObjects = new List<int>(),
            DeletedSteps = new List<int>(),
            NewName = "New Task Name",
            NewType = TaskType.ItemKit
        };
        
        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var expectedTask = new Endpoint.Res.ResTask(1, 1, "New Task Name", TaskType.ItemKit, 3, 3);
        
        Assert.NotNull(result);
        Assert.Equal(expectedTask, result.Task);
    }

    [Fact, Priority(10)]
    public async Task CanAddObject()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewObjects = new List<Endpoint.Req.NewObjectReq>()
                { new Endpoint.Req.NewObjectReq("newTestObjectName", new ObjectCoordinates()) },
            NewSteps = new List<Endpoint.Req.NewStepReq>(),
            ModifiedObjects = new List<Endpoint.Req.ModObjectReq>(),
            ModifiedSteps = new List<Endpoint.Req.ModStepReq>(),
            DeletedObjects = new List<int>(),
            DeletedSteps = new List<int>(),
            NewName = "New Task Name",
            NewType = TaskType.ItemKit
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(result);
        //there was 3 already, and one more added 3+1=4
        Assert.Equal(4, result.Task.NumObjects);
    }

    [Fact, Priority(20)]
    public async Task CanAddMultiplyObject()
    {

        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewObjects = new List<Endpoint.Req.NewObjectReq>(){new Endpoint.Req.NewObjectReq("newTestObjectName", new ObjectCoordinates()), new Endpoint.Req.NewObjectReq("newTestObjectName2", new ObjectCoordinates()), new Endpoint.Req.NewObjectReq("newTestObjectName3", new ObjectCoordinates())},
            NewSteps = new List<Endpoint.Req.NewStepReq>(),
            ModifiedObjects = new List<Endpoint.Req.ModObjectReq>(),
            ModifiedSteps = new List<Endpoint.Req.ModStepReq>(),
            DeletedObjects = new List<int>(),
            DeletedSteps = new List<int>(),
            NewName = "New Task Name",
            NewType = TaskType.ItemKit
        };
        
        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(result);
        //there was 3 alrady, below added one more, and now added 3 more 3+1+3=7
        Assert.Equal(7, result.Task.NumObjects);
    }
    
    [Fact, Priority(50)]
    public async Task CanDeleteObject()
    {
        Endpoint.Req reqDelete = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewObjects = new List<Endpoint.Req.NewObjectReq>(),
            NewSteps = new List<Endpoint.Req.NewStepReq>(),
            ModifiedObjects = new List<Endpoint.Req.ModObjectReq>(),
            ModifiedSteps = new List<Endpoint.Req.ModStepReq>(),
            DeletedObjects = new List<int>(){ 1, 2 },
            DeletedSteps = new List<int>(),
            NewName = "New Task Name",
            NewType = TaskType.ItemKit
        };
        
        var (responseDelete, resultDelete) = await _client.PUTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(reqDelete);
        
        Assert.NotNull(responseDelete);
        Assert.Equal(HttpStatusCode.OK, responseDelete.StatusCode);

        Assert.NotNull(resultDelete);
        //there was 3 alrady, below added 4 more, and now deleted 2 ot them 3+4-2=5
        Assert.Equal(5, resultDelete.Task.NumObjects);
    }
    
    [Fact, Priority(30)]
    public async Task CanAddStep()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewObjects = new List<Endpoint.Req.NewObjectReq>(),
            NewSteps = new List<Endpoint.Req.NewStepReq>()
                { new Endpoint.Req.NewStepReq(0, TemplateState.Missing, TemplateState.Present, 1), new Endpoint.Req.NewStepReq(0, TemplateState.Present, TemplateState.Missing, 1) },
            ModifiedObjects = new List<Endpoint.Req.ModObjectReq>(),
            ModifiedSteps = new List<Endpoint.Req.ModStepReq>(),
            DeletedObjects = new List<int>(),
            DeletedSteps = new List<int>(),
            NewName = "New Task Name",
            NewType = TaskType.ItemKit
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(result);
        // 5 bc seed adds 3 to it, and this adds 2 3+2=5
        Assert.Equal(5, result.Task.NumSteps);
    }
    
    [Fact, Priority(40)]
    public async Task CanDeleteStep()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewObjects = new List<Endpoint.Req.NewObjectReq>(),
            NewSteps = new List<Endpoint.Req.NewStepReq>(),
            ModifiedObjects = new List<Endpoint.Req.ModObjectReq>(),
            ModifiedSteps = new List<Endpoint.Req.ModStepReq>(),
            DeletedObjects = new List<int>(),
            DeletedSteps = new List<int>(){1, 2},
            NewName = "New Task Name",
            NewType = TaskType.ItemKit
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(result);
        // 7 bc seed adds 3 to it, the below test added 2 more, and this delete 2 3+2=5-2=3
        Assert.Equal(3, result.Task.NumSteps);
    }
    
    // [Fact]
    // public async Task CanModifySteps()
    // {
    //     Endpoint.Req req = new()
    //     {
    //         Id = 1,
    //         ParentJobId = 1,
    //         NewObjects = new List<Endpoint.Req.NewObjectReq>(),
    //         NewSteps = new List<Endpoint.Req.NewStepReq>(){new Endpoint.Req.NewStepReq(5, TemplateState.Missing, TemplateState.Present, 1)},
    //         ModifiedObjects = new List<Endpoint.Req.ModObjectReq>(),
    //         ModifiedSteps = new List<Endpoint.Req.ModStepReq>(){new Endpoint.Req.ModStepReq(5, 1, TemplateState.Missing, TemplateState.Present, 1)},
    //         DeletedObjects = new List<int>(),
    //         DeletedSteps = new List<int>(),
    //         NewName = "New Task Name",
    //         NewType = new TaskType()
    //     };
    //     
    //     var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);
    //     
    //     Assert.NotNull(response);
    //     Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    // }
    
    // [Fact]
    // public async Task CanModifyObjects()
    // {
    //     Endpoint.Req req = new()
    //     {
    //         Id = 1,
    //         ParentJobId = 1,
    //         NewObjects = new List<Endpoint.Req.NewObjectReq>(),
    //         NewSteps = new List<Endpoint.Req.NewStepReq>(){new Endpoint.Req.NewStepReq(5, TemplateState.Missing, TemplateState.Present, 1)},
    //         ModifiedObjects = new List<Endpoint.Req.ModObjectReq>(),
    //         ModifiedSteps = new List<Endpoint.Req.ModStepReq>(){new Endpoint.Req.ModStepReq(5, 1, TemplateState.Missing, TemplateState.Present, 1)},
    //         DeletedObjects = new List<int>(),
    //         DeletedSteps = new List<int>(),
    //         NewName = "New Task Name",
    //         NewType = new TaskType()
    //     };
    //     
    //     var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);
    //     
    //     Assert.NotNull(response);
    //     Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    // }
}