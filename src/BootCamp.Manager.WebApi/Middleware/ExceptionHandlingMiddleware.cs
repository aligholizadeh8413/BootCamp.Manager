using BootCamp.Manager.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
namespace BootCamp.Manager.WebApi.Middleware;
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        { context.Response.StatusCode = 499; }
        catch (Exception ex)
        {
            if (context.Response.HasStarted) throw;
            var status = 500;
            var message = "خطای داخلی رخ داد. دوباره تلاش کنید.";
            if (ex is AppException expected)
            { status = expected.StatusCode; message = expected.Message; }
            else if (ex is DbUpdateException { InnerException: SqliteException { SqliteErrorCode: 19 } })
            { status = 409; message = "عملیات با روابط یا محدودیت‌های دیتابیس تداخل دارد."; }
            if (status == 500) logger.LogError(ex, "Unhandled exception; Trace {Trace}", context.TraceIdentifier);
            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails {
                Status = status, Title = message, Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            }, options: null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
        }
    }
}
