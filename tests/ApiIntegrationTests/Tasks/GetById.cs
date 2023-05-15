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
        Assert.Null(result.OngoingInstance);

        Endpoint.Req req3 = new()
        {
            Id = 3
        };

        var (response3, result3) = await _client.GETAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req3);

        Assert.NotNull(response3);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        Assert.NotNull(result3);
        Assert.Equal(3, result3.Id);
        Assert.Equal("Task 3", result3.Name);
        Assert.NotNull(result3.OngoingInstance);
    }
}