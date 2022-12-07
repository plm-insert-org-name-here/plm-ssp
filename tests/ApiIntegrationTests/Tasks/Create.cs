using System.Net;
using Api;
using FastEndpoints;
using Xunit;
using Endpoint = Api.Endpoints.Tasks.Create;

namespace ApiIntegrationTests.Tasks;

[Collection("Sequential")]
public class Create : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public Create(Setup setup)
    {
        _client = setup.Client;
    }

    [Fact]
    public async Task CanCreate()
    {
        Endpoint.Req req = new()
        {
            Name = "New Task",
            ParentJobId = 1,
            LocationId = 1
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(4, result.Id);
        Assert.Equal("New Task", result.Name);
    }

    [Fact]
    public async Task BadRequestForNoSnapshot()
    {
        Endpoint.Req req = new()
        {
            Name = "New Task 2",
            ParentJobId = 1,
            LocationId = 2
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CantCreateWithInvalidParentId()
    {
        Endpoint.Req req = new()
        {
            Name = "New Task 3",
            ParentJobId = 10,
            LocationId = 1
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CantCreateWithInvalidLocationId()
    {
        Endpoint.Req req = new()
        {
            Name = "New Task 3",
            ParentJobId = 1,
            LocationId = 56
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // [Fact]
    // public async Task CantCreateWithDuplicateNameWithinParent()
    // {
    //     Endpoint.Req req = new()
    //     {
    //         Name = "Task 1",
    //         ParentJobId = 1,
    //         LocationId = 1
    //     };
    //
    //     var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);
    //
    //     Assert.NotNull(response);
    //     Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    // }
}