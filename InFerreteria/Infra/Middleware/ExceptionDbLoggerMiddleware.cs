using Microsoft.AspNetCore.Http;
using WebApp.Infra.Services;

// /Infra/Middleware/ExceptionDbLoggerMiddleware.cs
namespace WebApp.Infra.Middleware;

public class ExceptionDbLoggerMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionDbLoggerMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx, IErrorLogger logger)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await logger.LogExceptionAsync(ex, ctx);
            throw; // re-lanza para que tu pipeline/DeveloperExceptionPage actúe
        }
    }
}
