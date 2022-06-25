using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.BackgroundJobs;

public static class HttpBackgroundJob
{
    /// <summary>
    /// Executes a background job in an isolated <see cref="ShellScope"/> after the current HTTP request is completed.
    /// </summary>
    public static Task AfterEndOfRequestAsync(Func<ShellScope, Task> job)
    {
        var scope = ShellScope.Current;

        // Can't be executed e.g. during a tenant setup.
        if (scope.ShellContext.Settings.State != TenantState.Running)
        {
            return Task.CompletedTask;
        }

        // Can't be executed outside the context of a real http request scope.
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        if (httpContextAccessor.HttpContext == null || httpContextAccessor.HttpContext.Items.TryGetValue("IsBackground", out _))
        {
            return Task.CompletedTask;
        }

        // Fire and forget in an isolated child scope.
        _ = ShellScope.UsingChildScopeAsync(async scope =>
        {
            var timeoutTask = Task.Delay(60_000);

            // Wait for the current 'HttpContext' to be released with a timeout of 60s.
            while (httpContextAccessor.HttpContext != null)
            {
                await Task.Delay(1_000);
                if (timeoutTask.IsCompleted)
                {
                    return;
                }
            }

            httpContextAccessor.HttpContext = scope.ShellContext.CreateHttpContext();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ShellScope>>();
            try
            {
                await job(scope);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while executing a background job after the end of a request on tenant '{TenantName}'.",
                    scope.ShellContext.Settings.Name);
            }
        });

        return Task.CompletedTask;
    }
}
