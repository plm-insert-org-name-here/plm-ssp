using System.Net;
using FastEndpoints;
using Xunit;
using Endpoint = Api.Endpoints.Tasks.GetInstance;

namespace ApiIntegrationTests.Tasks;

[Collection("Sequential")]
public class GetInstance : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public GetInstance(Setup setup)
    {
        _client = setup.Client;
    }
    
    [Fact]
    public async Task CanGetActive()
    {
        Endpoint.Req req = new()
        {
            TaskId = 1
        };

        var (response, result) = await _client.GETAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(2, result.Instance.Id);
    }
    
    [Fact]
    public async Task CantGetIfNoActive()
    {
        Endpoint.Req req = new()
        {
            TaskId = 2
        };

        var (response, result) = await _client.GETAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}