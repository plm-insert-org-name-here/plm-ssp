using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Api.Services.DetectorController
{
    public class DetectorControllerMiddleware
    {
        private readonly RequestDelegate _next;

        public DetectorControllerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DetectorController detectorController)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
            }
            else if (context.Request.Path == Routes.Detectors.Controller)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await detectorController.HandleConnection(webSocket);
                // TODO(rg): should HandleConnection return something?
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
    }
}