using System.Net;
using Domain.Common;
using FastEndpoints;
using Xunit;
using Endpoint = Api.Endpoints.Tasks.GetById;

namespace ApiIntegrationTests.Tasks;

[Collection("Sequential")]
public class GetById : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public GetById(Setup setup)
    {
        _client = setup.Client;
    }
    
    [Fact]
    public async Task CanGetById()
    {
        Endpoint.Req req = new()
        {
            Id = 1
        };

        var (response, result) = await _client.GETAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Task 1", result.Name);
        Assert.Equal(TaskState.Active, result.State);
        
        Endpoint.Req req2 = new()
        {
            Id = 2
        };

        var (response2, result2) = await _client.GETAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req2);
        
        Assert.NotNull(response2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        Assert.NotNull(result2);
        Assert.Equal(2, result2.Id);
        Assert.Equal("Task 2", result2.Name);
        Assert.Equal(TaskState.Inactive, result2.State);
    }
}