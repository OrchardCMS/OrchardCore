using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Shell;

public class ShellScopeTests
{
    [Fact]
    public static async Task ShellScopeConcurrencyTest()
    {
        var context = new SiteContext()
            .WithRecipe("SaaS");
        await context.InitializeAsync();

        var waitHandle = new ManualResetEventSlim();
        var cts = new CancellationTokenSource();

        var t1 = context.UsingTenantScopeAsync(scope =>
        {
            scope.RegisterBeforeDispose(async innerScope =>
            {
                waitHandle.Set();

                // Simulate some work in the before dispose action. Must be longer than the duration of the second
                // scope, to ensure the second scope is done before this action completes.
                try
                {
                    await Task.Delay(1000, cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }

                // Ensure the ShellContext is still alive at this point, as the second scope must not dispose it.
                Assert.False(innerScope.ShellContext.IsDisposed, "The shell context should not be disposed yet.");
            });

            return Task.CompletedTask;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            waitHandle.Wait();

            var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
            var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();

            // Release shell context from the second scope. This should not cause any issues in the first
            // scope.
            await shellHost.ReleaseShellContextAsync(shellSettings).ConfigureAwait(false);
        });

        // Continue the first scope after the second scope has released the shell context.
        cts.Cancel();

        await t1;
    }
}
