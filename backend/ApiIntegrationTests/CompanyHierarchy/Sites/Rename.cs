using System.Net;
using FastEndpoints;
using Xunit;
using Xunit.Priority;
using Endpoint = Api.Endpoints.Sites.Rename;

namespace ApiIntegrationTests.CompanyHierarchy.Sites;

[Collection("Sequential")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class Rename : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public Rename(Setup setup)
    {
        _client = setup.Client;
    }

    [Fact]
    public async Task CanRename()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            Name = "New Site"
        };
        
        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CantRenameNonexistent()
    {
        Endpoint.Req req = new()
        {
            Id = 1000,
            Name = "New Site"
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CantRenameToDuplicateGlobally()
    {
        Endpoint.Req req = new()
        {
            Id = 1,
            Name = "Site 2"
        };

        var (response, result) = await _client.PUTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}