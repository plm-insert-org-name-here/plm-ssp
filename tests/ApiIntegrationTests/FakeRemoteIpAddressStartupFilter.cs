namespace ApiIntegrationTests;

public class FakeIpAddressStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<FakeRemoteIpAddressMiddleware>();
            next(app);
        };
    }
}