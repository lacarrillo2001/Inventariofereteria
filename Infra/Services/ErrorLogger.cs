using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebApp.Data;
using WebApp.Data.Infra.Entities;
// /Infra/Services/ErrorLogger.cs
namespace WebApp.Infra.Services;

public interface IErrorLogger
{
    Task LogExceptionAsync(Exception ex, HttpContext ctx, string? controller = null, string? action = null);
    Task LogValidationAsync(string message, HttpContext ctx, object? modelStateDump = null, string level = "Warning");
}

public class ErrorLogger : IErrorLogger
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _http;

    public ErrorLogger(ApplicationDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task LogExceptionAsync(Exception ex, HttpContext ctx, string? controller = null, string? action = null)
    {
        var log = new ErrorLog
        {
            Level = "Error",
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Controller = controller,
            Action = action,
            UserName = ctx?.User?.FindFirstValue(ClaimTypes.Name) ?? ctx?.User?.Identity?.Name,
            Path = ctx?.Request?.Path.Value,
            QueryString = ctx?.Request?.QueryString.Value,
            CreatedAt = DateTime.UtcNow
        };
        await _db.ErrorLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }

    public async Task LogValidationAsync(string message, HttpContext ctx, object? modelStateDump = null, string level = "Warning")
    {
        var json = modelStateDump is null ? null : JsonSerializer.Serialize(modelStateDump);
        var log = new ErrorLog
        {
            Level = level,
            Message = message,
            FormJson = json,
            UserName = ctx?.User?.FindFirstValue(ClaimTypes.Name) ?? ctx?.User?.Identity?.Name,
            Path = ctx?.Request?.Path.Value,
            QueryString = ctx?.Request?.QueryString.Value,
            CreatedAt = DateTime.UtcNow
        };
        await _db.ErrorLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }
}
