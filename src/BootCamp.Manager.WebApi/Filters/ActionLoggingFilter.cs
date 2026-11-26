using Microsoft.AspNetCore.Mvc.Filters;
namespace BootCamp.Manager.WebApi.Filters;
public class ActionLoggingFilter(ILogger<ActionLoggingFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogInformation("Action starting: {Action}", context.ActionDescriptor.DisplayName);
        var result = await next();
        logger.LogInformation("Action finished: {Action}; Failed: {Failed}",
            context.ActionDescriptor.DisplayName, result.Exception is not null);
    }
}
