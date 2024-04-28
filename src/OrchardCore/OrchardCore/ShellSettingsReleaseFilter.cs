using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore;

public class ShellSettingsReleaseFilter : IResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context)
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var shellContextReleaseService = scope.ServiceProvider.GetService<IShellContextReleaseService>();

            await shellContextReleaseService.ProcessAsync();
        });
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
    }
}
