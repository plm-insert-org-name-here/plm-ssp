using System.Net;
using FastEndpoints;
using Xunit;
using Xunit.Priority;
using Endpoint = Api.Endpoints.Lines.Create;

namespace ApiIntegrationTests.CompanyHierarchy.Lines;

[Collection("Sequential")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class Create : IClassFixture<Setup>
{
    private readonly HttpClient _client;


    public Create(Setup setup)
    {
        _client = setup.Client;
    }

    [Fact, Priority(0)]
    public async Task CanCreate()
    {
        Endpoint.Req req = new()
        {
            Name = "New Line",
            OPUId = 1
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("New Line", result.Name);
    }

    [Fact, Priority(10)]
    public async Task CantCreateWithInvalidParentId()
    {
        Endpoint.Req req = new()
        {
            Name = "New Line 2",
            OPUId = 10
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Priority(20)]
    public async Task CantCreateWithDuplicateNameWithinParent()
    {
        Endpoint.Req req = new()
        {
            Name = "Line 2-2",
            OPUId = 2
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Priority(30)]
    public async Task CanCreateWithDuplicateNameAcrossParents()
    {
        Endpoint.Req req = new()
        {
            Name = "Line 2-2",
            OPUId = 1
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}