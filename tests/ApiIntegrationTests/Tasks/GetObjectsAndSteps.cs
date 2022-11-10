using System.Net;
using FastEndpoints;
using Xunit;
using Endpoint = Api.Endpoints.Tasks.GetObjectsAndSteps;

namespace ApiIntegrationTests.Tasks;

[Collection("Sequential")]
public class GetObjectsAndSteps : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public GetObjectsAndSteps(Setup setup)
    {
        _client = setup.Client;
    }
    
    [Fact]
    public async Task CanGet()
    {
        Endpoint.Req req = new()
        {
            TaskId = 1
        };

        var (response, result) = await _client.GETAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(3, result.Objects.Count());
        Assert.Equal(3, result.Steps.Count());

    }
    
    [Fact]
    public async Task CantGetIfTaskInvalid()
    {
        Endpoint.Req req = new()
        {
            TaskId = 3
        };

        var (response, result) = await _client.GETAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    }
}