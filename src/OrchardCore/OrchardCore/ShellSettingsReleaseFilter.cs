using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Infrastructure;

namespace OrchardCore;

public class ShellSettingsReleaseFilter : IResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context)
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();

            if (httpContextAccessor?.HttpContext != null &&
            httpContextAccessor.HttpContext.Items.TryGetValue(nameof(ShellSettingsReleaseRequest), out var request) &&
            request is ShellSettingsReleaseRequest shellSettingsReleaseRequest &&
            shellSettingsReleaseRequest.Release)
            {
                var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
                var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();

                await shellHost.ReleaseShellContextAsync(shellSettings);
            }
        });
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
    }
}
