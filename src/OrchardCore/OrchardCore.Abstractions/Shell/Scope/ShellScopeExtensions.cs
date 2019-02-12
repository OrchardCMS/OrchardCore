using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DeferredTasks;

namespace OrchardCore.Environment.Shell.Scope
{
    public static class ShellScopeExtensions
    {
        public static async Task UsingAsync(this ShellScope scope, Func<ShellScope, Task> execute)
        {
            if (scope == null)
            {
                await execute(ShellScope.Current);
                return;
            }

            var shellContext = scope.ShellContext;
            var shellSettings = shellContext.Settings;

            var shellHost = shellContext.ServiceProvider.GetRequiredService<IShellHost>();

            var hasPendingTasks = false;

            using (scope)
            {
                await scope.ActivateAsync();

                await execute(scope);

                await scope.OnCompletedAsync();

                var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();
                hasPendingTasks = deferredTaskEngine?.HasPendingTasks ?? false;
            }

            // Create a new scope only if there are pending tasks
            if (hasPendingTasks)
            {
                using (var pendingScope = await shellHost.GetScopeAsync(shellSettings))
                {
                    var deferredTaskEngine = pendingScope.ServiceProvider.GetService<IDeferredTaskEngine>();
                    var context = new DeferredTaskContext(pendingScope.ServiceProvider);

                    await deferredTaskEngine.ExecuteTasksAsync(context);
                    await pendingScope.OnCompletedAsync();
                }
            }
        }
    }
}
