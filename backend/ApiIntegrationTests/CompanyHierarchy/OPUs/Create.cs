using System.Net;
using FastEndpoints;
using Xunit;
using Xunit.Priority;
using Endpoint = Api.Endpoints.OPUs.Create;

namespace ApiIntegrationTests.CompanyHierarchy.OPUs;

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
            Name = "New OPU",
            ParentSiteId = 1
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("New OPU", result.Name);
    }

    [Fact, Priority(10)]
    public async Task CantCreateWithInvalidParentId()
    {
        Endpoint.Req req = new()
        {
            Name = "New OPU 2",
            ParentSiteId = 10
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
            Name = "OPU 2-2",
            ParentSiteId = 2
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
            Name = "OPU 2-2",
            ParentSiteId = 1
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
}