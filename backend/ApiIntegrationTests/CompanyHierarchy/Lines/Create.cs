using System.Net;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;
using Xunit;
using Endpoint = Api.Endpoints.Lines.Create;

namespace ApiIntegrationTests.CompanyHierarchy.Lines;

public class Create : IClassFixture<Setup>
{
    private readonly IRepository<OPU> _opuRepo;
    private readonly ICHNameUniquenessChecker<OPU, Line> _nameUniquenessChecker;
    private readonly HttpClient _client;


    public Create(Setup setup)
    {
        _client = setup.Client;

        var scope = setup.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _opuRepo = scope.ServiceProvider.GetRequiredService<IRepository<OPU>>();
        _nameUniquenessChecker = scope.ServiceProvider.GetRequiredService<ICHNameUniquenessChecker<OPU, Line>>();
    }

    [Fact]
    public async Task CanCreate()
    {
        Endpoint.Req req = new()
        {
            Name = "New Line",
            OPUId = 1
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("New Line", result.Name);
    }

    [Fact]
    public async Task CantCreateWithInvalidParentId()
    {
        Endpoint.Req req = new()
        {
            Name = "New Line 2",
            OPUId = 10
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, EmptyResponse>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CantCreateWithDuplicateNameWithinParent()
    {
        Endpoint.Req req = new()
        {
            Name = "Line 2-2",
            OPUId = 2
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CanCreateWithDuplicateNameAcrossParents()
    {
        Endpoint.Req req = new()
        {
            Name = "Line 2-2",
            OPUId = 1
        };

        var (response, result) = await _client.POSTAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}