using System.Net;
using FastEndpoints;
using Xunit;
using Endpoint = Api.Endpoints.Tasks.Delete;

namespace ApiIntegrationTests.Tasks;

[Collection("Sequential")]
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

        var (response, result) = await _client.DELETEAsync<Api.Endpoints.Tasks.Delete, Endpoint.Req, EmptyResponse>(req);

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
        
        var (response, result) = await _client.DELETEAsync<Api.Endpoints.Tasks.Delete, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}