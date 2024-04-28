using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore;

public class ShellSettingsReleaseFilter : IResultFilter
{
    private readonly IShellContextReleaseService _shellContextReleaseService;

    public ShellSettingsReleaseFilter(IShellContextReleaseService shellContextReleaseService)
    {
        _shellContextReleaseService = shellContextReleaseService;
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            // var shellContextReleaseService = scope.ServiceProvider.GetService<IShellContextReleaseService>();

            await _shellContextReleaseService.ProcessAsync();
        });
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
    }
}
