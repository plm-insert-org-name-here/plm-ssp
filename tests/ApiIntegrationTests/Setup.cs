using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace ApiIntegrationTests;

public class Setup
{
    private WebApplicationFactory<Program> Application { get; }

    public HttpClient Client => Application.CreateClient();
    public IServiceProvider Services { get; }

    public Setup()
    {
        Application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(x =>
            {
                // We need to use a fake detector connection to imitate that there's a working detector we're talking to
                x.AddScoped<IDetectorConnection, MockedDetectorConnection>();

                // WAF test requests won't have an IP address, but some of the handlers expect the remote IP address
                // to exist, so we specify it manually
                x.AddSingleton<IStartupFilter, FakeIpAddressStartupFilter>();
            });

            // This makes it so the app settings files are read from the integration tests' project,
            // instead of the Api project
            builder.UseSolutionRelativeContentRoot("tests/ApiIntegrationTests");
        });

        Services = Application.Services;
    }
}