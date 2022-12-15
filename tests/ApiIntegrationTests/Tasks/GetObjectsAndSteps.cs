using System.Net;
using FastEndpoints;
using Xunit;

namespace ApiIntegrationTests.Tasks;
using Endpoint = Api.Endpoints.Tasks.GetObjectsAndSteps;

[Collection("Sequential")]
public class GetObjectsAndStep : IClassFixture<Setup>
{
    private readonly HttpClient _client;
    
    public GetObjectsAndStep(Setup setup)
    {
        _client = setup.Client;
    }

    [Fact]
    public async Task GetObjectsAndSteps()
    {
        Endpoint.Req req = new()
        {
            TaskId = 1
        };

        var (response, result) = await _client.GETAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.Equal(3,result.Objects.Count());
        Assert.Equal(3, result.Steps.Count());
        
        Endpoint.Req req2 = new()
        {
            TaskId = 3
        };

        var (response2, result2) = await _client.GETAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req2);
        
        Assert.NotNull(response2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        Assert.Equal(3,result2.Objects.Count());
        Assert.Equal(6, result2.Steps.Count());
    }

    [Fact]
    public async Task GetObjectsAndStepsFailIfInvalidTaskId()
    {
        Endpoint.Req req = new()
        {
            TaskId = 1123213
        };
        
        var (response, result) = await _client.GETAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}