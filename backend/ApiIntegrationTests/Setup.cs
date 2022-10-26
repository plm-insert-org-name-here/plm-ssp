using System.Text;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ApiIntegrationTests;

public class Setup
{
    private WebApplicationFactory<Program> Application { get; }

    public HttpClient Client => Application.CreateClient();
    public IServiceProvider Services => Application.Services;

    public Setup()
    {
        Application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.Configure<DbOpt>(opt =>
                {
                    opt.ConnectionString = "server=localhost;user=plm;password=kiskacsa;database=plm_ssp_testing";
                    opt.SeedFolderRelativePath = "../ApiIntegrationTests/TestData";
                    opt.DeleteFirst = true;
                });

            });
        });
    }
}