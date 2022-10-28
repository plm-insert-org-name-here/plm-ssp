using System.Net;
using FastEndpoints;
using Xunit;
using Xunit.Priority;
using Endpoint = Api.Endpoints.Sites.List;

namespace ApiIntegrationTests.CompanyHierarchy.Sites;

[Collection("Sequential")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class List : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public List(Setup setup)
    {
        _client = setup.Client;
    }

    [Fact]
    public async Task CanList()
    {
        var (response, result) = await _client.GETAsync<Endpoint, EmptyRequest, IEnumerable<Endpoint.Res>>(new EmptyRequest());
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}