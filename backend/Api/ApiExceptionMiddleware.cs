using FastEndpoints;
using Infrastructure.Exceptions;

namespace Api;

public class ApiExceptionResponse
{
    public int StatusCode { get; set; }
    public string Description { get; set; } = default!;
    public IEnumerable<string>? Errors { get; set; }
}

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ApiExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainFailureException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var res = new ApiExceptionResponse
            {
                StatusCode = context.Response.StatusCode,
                Description = "One or more domain errors occurred.",
                Errors = ex.Errors.Select(e => e.Message)
            };

            await context.Response.WriteAsJsonAsync(res);
        }
        catch (ValidationFailureException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var res = new ApiExceptionResponse
            {
                StatusCode = context.Response.StatusCode,
                Description = "One or more validation errors occurred.",
                Errors = ex.Failures?.Select(e => e.ErrorMessage)
            };

            await context.Response.WriteAsJsonAsync(res);
        }
        catch (Exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var res = new ApiExceptionResponse()
            {
                StatusCode = context.Response.StatusCode,
                Description = "An unknown server error occurred."
            };

            await context.Response.WriteAsJsonAsync(res);

            throw;
        }
    }
}