using System.Net;
using FastEndpoints;
using Xunit;
using Endpoint = Api.Endpoints.Lines.GetById;

namespace ApiIntegrationTests.CompanyHierarchy.Lines;

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
        Assert.Equal(2, result.Stations.Count());
    }
}