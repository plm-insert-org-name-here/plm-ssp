using System.Net;
using FastEndpoints;
using Xunit;
using Xunit.Priority;
using Endpoint = Api.Endpoints.Sites.Delete;

namespace ApiIntegrationTests.CompanyHierarchy.Sites;

[Collection("Sequential")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class Delete : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public Delete(Setup setup)
    {
        _client = setup.Client;
    }
    
    [Fact]
    public async Task CanDelete()
    {
        Endpoint.Req req = new()
        {
            Id = 1
        };

        var (response, result) = await _client.DELETEAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CantDeleteNonexistent()
    {
        Endpoint.Req req = new()
        {
            Id = 1000
        };
        
        var (response, result) = await _client.DELETEAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}