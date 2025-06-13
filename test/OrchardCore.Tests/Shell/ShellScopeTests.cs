using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
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
                    await Task.Delay(1000, cts.Token);
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
            await shellHost.ReleaseShellContextAsync(shellSettings);
        });

        // Continue the first scope after the second scope has released the shell context.
        cts.Cancel();

        await t1;
    }

    // When tenant activation is called recursively, this test will be blocking forever. The timeout is
    // set to 10 seconds to ensure that the test fails if the activation is not handled correctly.
    [Fact(Timeout = 10000)]
    public async Task TenantActivationIsNotInvokedRecursivelyTest()
    {
        var site = new OrchardTestFixture<ShellScopeStartup>();
        var shellSettings = new ShellSettings()
        {
            Name = Guid.NewGuid().ToString("N"),
            RequestUrlPrefix = "tenant1",
        }
        .AsUninitialized();

        var shellHost = site.Services.GetRequiredService<IShellHost>();

        await shellHost.InitializeAsync();
        var shellContext = await shellHost.GetOrCreateShellContextAsync(shellSettings);

        var outerScope = await shellHost.GetScopeAsync(shellSettings);

        await outerScope.UsingAsync(async scope =>
        {
            // Simulate some work in the outer scope.
            await Task.Delay(1);
        }, activateShell: true);

        var testTenantEvents = site.Services.GetRequiredService<Mock<IModularTenantEvents>>();

        testTenantEvents.Verify(x => x.ActivatingAsync(), Times.Once);
        testTenantEvents.Verify(x => x.ActivatedAsync(), Times.Once);
    }

    private class ShellScopeStartup
    {
        private readonly SiteStartup _siteStartup = new SiteStartup();

        public void ConfigureServices(IServiceCollection services)
        {
            _siteStartup.ConfigureServices(services);

            var inScope = false;
            var tenantEvents = new Mock<IModularTenantEvents>();
            tenantEvents.Setup(x => x.ActivatingAsync())
                .Returns(async () =>
                {
                    if (inScope)
                    {
                        return;
                    }

                    inScope = true;
                    await ShellScope.UsingChildScopeAsync(async innerScope =>
                    {
                        Assert.False(innerScope.ShellContext.IsActivated, "The shell context should not be activated yet");

                        // Simulate some work in the inner scope.
                        await Task.Delay(1);
                    }, activateShell: true);

                    inScope = false;
                });

            services.AddSingleton(tenantEvents);
            services.AddSingleton(sp => tenantEvents.Object);
        }

        public void Configure(IApplicationBuilder app)
        {
            _siteStartup.Configure(app);
        }
    }
}
