using System.Diagnostics;
namespace BootCamp.Manager.WebApi.Middleware;
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        try { await next(context); }
        finally
        {
            logger.LogInformation("HTTP {Method} {Path} => {Status} in {Elapsed} ms; Trace {Trace}",
                context.Request.Method, context.Request.Path, context.Response.StatusCode,
                timer.ElapsedMilliseconds, context.TraceIdentifier);
        }
    }
}
