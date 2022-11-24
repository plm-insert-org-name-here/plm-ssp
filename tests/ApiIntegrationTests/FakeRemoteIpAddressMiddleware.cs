using System.Net;

namespace ApiIntegrationTests;

public class FakeRemoteIpAddressMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPAddress _fakeIpAddress = IPAddress.Parse("127.0.0.1");

    public FakeRemoteIpAddressMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        httpContext.Connection.RemoteIpAddress = _fakeIpAddress;

        await _next(httpContext);
    }
}