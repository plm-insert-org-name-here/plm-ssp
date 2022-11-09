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
            NewName = "New Task Name",
            NewType = TaskType.ItemKit
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact, Priority(10)]
    public async Task CanAddObject()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewObjects = new List<Endpoint.Req.NewObjectReq> { new("newTestObjectName", new ObjectCoordinates()) },
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact, Priority(20)]
    public async Task CanAddMultipleObjects()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewObjects = new List<Endpoint.Req.NewObjectReq>
            {
                new("newTestObjectName2",
                    new ObjectCoordinates {X = 10, Y = 10, Width = 100, Height = 100}),
                new("newTestObjectName3",
                    new ObjectCoordinates { X = 20, Y = 20, Width = 200, Height = 200}),
                new("newTestObjectName4",
                    new ObjectCoordinates { X = 30, Y = 30, Width = 300, Height = 300})
            }
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact, Priority(50)]
    public async Task CanDeleteObjects()
    {
        Endpoint.Req reqDelete = new()
        {
            Id = 1,
            ParentJobId = 1,
            DeletedObjects = new List<int>{ 1, 2 },
        };

        var (responseDelete, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(reqDelete);

        Assert.NotNull(responseDelete);
        Assert.Equal(HttpStatusCode.NoContent, responseDelete.StatusCode);
    }

    [Fact, Priority(30)]
    public async Task CanAddStep()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            NewSteps = new List<Endpoint.Req.NewStepReq>
                { new(0, TemplateState.Missing, TemplateState.Present, "Object 1"), new(0, TemplateState.Present, TemplateState.Missing, "Object 2") },
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact, Priority(40)]
    public async Task CanDeleteSteps()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            ParentJobId = 1,
            DeletedSteps = new List<int>{1, 2},
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
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