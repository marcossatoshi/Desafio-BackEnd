using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Mottu.Rentals.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, ex);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var (status, title) = MapException(ex);

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = ex.Message,
            Type = $"https://httpstatuses.com/{(int)status}"
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status!.Value;
        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode status, string title) MapException(Exception ex)
    {
        if (ex is InvalidOperationException ioe)
        {
            var msg = ioe.Message.ToLowerInvariant();
            if (msg.Contains("already exists") || msg.Contains("already rented") || msg.Contains("has active rental"))
                return (HttpStatusCode.Conflict, "Conflict");
            if (msg.Contains("not found"))
                return (HttpStatusCode.NotFound, "Not Found");
            if (msg.Contains("not allowed") || msg.Contains("invalid"))
                return (HttpStatusCode.BadRequest, "Bad Request");
            return (HttpStatusCode.BadRequest, "Bad Request");
        }

        return (HttpStatusCode.InternalServerError, "Internal Server Error");
    }
}


