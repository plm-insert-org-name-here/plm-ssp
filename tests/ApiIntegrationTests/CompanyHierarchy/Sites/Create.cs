using System.Net;
using FastEndpoints;
using Xunit;
using Xunit.Priority;
using Endpoint = Api.Endpoints.Sites.Create;

namespace ApiIntegrationTests.CompanyHierarchy.Sites;

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
            Name = "New Site",
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("New Site", result.Name);
    }

    [Fact, Priority(10)]
    public async Task CantCreateWithDuplicateNameGlobally()
    {
        Endpoint.Req req = new()
        {
            Name = "Site 1",
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}