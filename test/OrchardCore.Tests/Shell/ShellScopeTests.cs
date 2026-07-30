using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
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

    [Fact]
    public async Task ActivateShell_StartupValidators_ExecuteBetweenTenantEvents()
    {
        var executionOrder = new List<string>();
        await using var shellContext = CreateShellContext(services =>
        {
            services.AddSingleton<IModularTenantEvents>(new RecordingTenantEvents(executionOrder));
            services.AddOptions<TestOptions>()
                .Configure(options => options.IsValid = true)
                .Validate(options =>
                {
                    executionOrder.Add("Sync validation");
                    return options.IsValid;
                })
                .Validate((options, cancellationToken) =>
                {
                    executionOrder.Add("Async validation");
                    return Task.FromResult(options.IsValid);
                })
                .ValidateOnStart();
        });

        await ActivateAsync(shellContext);

        Assert.True(shellContext.IsActivated);
        Assert.Equal(
            ["Activating", "Sync validation", "Async validation", "Activated"],
            executionOrder);
    }

    [Fact]
    public async Task ActivateShell_SyncStartupValidationFails_DoesNotActivateAndCanRetry()
    {
        var validator = new MutableStartupValidator { ShouldFail = true };
        await using var shellContext = CreateShellContext(services =>
        {
            services.AddSingleton<IStartupValidator>(validator);
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => ActivateAsync(shellContext));

        Assert.False(shellContext.IsActivated);
        Assert.Equal(1, validator.CallCount);

        validator.ShouldFail = false;
        await ActivateAsync(shellContext);

        Assert.True(shellContext.IsActivated);
        Assert.Equal(2, validator.CallCount);
    }

    [Fact]
    public async Task ActivateShell_AsyncStartupValidationFails_DoesNotActivate()
    {
        var validator = new RecordingAsyncStartupValidator(_ => throw new OptionsValidationException(
            Options.DefaultName,
            typeof(TestOptions),
            ["Async validation failed."]));
        await using var shellContext = CreateShellContext(services =>
        {
            services.AddSingleton<IAsyncStartupValidator>(validator);
        });

        await Assert.ThrowsAsync<OptionsValidationException>(() => ActivateAsync(shellContext));

        Assert.False(shellContext.IsActivated);
        Assert.Equal(1, validator.CallCount);
    }

    [Fact]
    public async Task ActivateShell_AsyncStartupValidation_UsesApplicationStoppingToken()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var validator = new RecordingAsyncStartupValidator(cancellationToken =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        });
        await using var shellContext = CreateShellContext(services =>
        {
            services.AddSingleton<IHostApplicationLifetime>(new TestHostApplicationLifetime(cancellationTokenSource.Token));
            services.AddSingleton<IAsyncStartupValidator>(validator);
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ActivateAsync(shellContext));

        Assert.False(shellContext.IsActivated);
        Assert.Equal(cancellationTokenSource.Token, validator.CancellationToken);
    }

    [Fact]
    public async Task ActivateShell_ActivatedShell_DoesNotRunStartupValidatorsAgain()
    {
        var syncValidator = new MutableStartupValidator();
        var validatorStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var completeValidator = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var asyncValidator = new RecordingAsyncStartupValidator(_ =>
        {
            validatorStarted.TrySetResult();
            return completeValidator.Task;
        });
        await using var shellContext = CreateShellContext(services =>
        {
            services.AddSingleton<IStartupValidator>(syncValidator);
            services.AddSingleton<IAsyncStartupValidator>(asyncValidator);
        });

        var firstActivation = ActivateAsync(shellContext);
        await validatorStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        var secondActivation = ActivateAsync(shellContext);
        completeValidator.SetResult();
        await Task.WhenAll(firstActivation, secondActivation);

        Assert.Equal(1, syncValidator.CallCount);
        Assert.Equal(1, asyncValidator.CallCount);
    }

    [Fact]
    public async Task ActivateShell_RebuiltShell_RunsStartupValidatorsAgain()
    {
        var syncValidator = new MutableStartupValidator();
        var asyncValidator = new RecordingAsyncStartupValidator(_ => Task.CompletedTask);

        await using (var firstShellContext = CreateShellContext(services =>
        {
            services.AddSingleton<IStartupValidator>(syncValidator);
            services.AddSingleton<IAsyncStartupValidator>(asyncValidator);
        }))
        {
            await ActivateAsync(firstShellContext);
        }

        await using (var rebuiltShellContext = CreateShellContext(services =>
        {
            services.AddSingleton<IStartupValidator>(syncValidator);
            services.AddSingleton<IAsyncStartupValidator>(asyncValidator);
        }))
        {
            await ActivateAsync(rebuiltShellContext);
        }

        Assert.Equal(2, syncValidator.CallCount);
        Assert.Equal(2, asyncValidator.CallCount);
    }

    private static async Task ActivateAsync(ShellContext shellContext)
    {
        var scope = await shellContext.CreateScopeAsync();
        await scope.UsingAsync(_ => Task.CompletedTask);
    }

    private static ShellContext CreateShellContext(Action<IServiceCollection> configureServices)
    {
        var services = new ServiceCollection();
        var localLock = new LocalLock(NullLogger<LocalLock>.Instance);
        services.AddSingleton<ILocalLock>(localLock);
        services.AddSingleton<IDistributedLock>(localLock);
        configureServices(services);

        return new ShellContext
        {
            Settings = new ShellSettings { Name = "Test" }.AsInitializing(),
            ServiceProvider = services.BuildServiceProvider(),
        };
    }

    private sealed class TestOptions
    {
        public bool IsValid { get; set; }
    }

    private sealed class RecordingTenantEvents : ModularTenantEvents
    {
        private readonly List<string> _executionOrder;

        public RecordingTenantEvents(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public override Task ActivatingAsync()
        {
            _executionOrder.Add("Activating");
            return Task.CompletedTask;
        }

        public override Task ActivatedAsync()
        {
            _executionOrder.Add("Activated");
            return Task.CompletedTask;
        }
    }

    private sealed class MutableStartupValidator : IStartupValidator
    {
        public bool ShouldFail { get; set; }

        public int CallCount { get; private set; }

        public void Validate()
        {
            CallCount++;

            if (ShouldFail)
            {
                throw new InvalidOperationException("Sync validation failed.");
            }
        }
    }

    private sealed class RecordingAsyncStartupValidator : IAsyncStartupValidator
    {
        private readonly Func<CancellationToken, Task> _validateAsync;

        public RecordingAsyncStartupValidator(Func<CancellationToken, Task> validateAsync)
        {
            _validateAsync = validateAsync;
        }

        public int CallCount { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task ValidateAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            CancellationToken = cancellationToken;
            return _validateAsync(cancellationToken);
        }
    }

    private sealed class TestHostApplicationLifetime : IHostApplicationLifetime
    {
        public TestHostApplicationLifetime(CancellationToken applicationStopping)
        {
            ApplicationStopping = applicationStopping;
        }

        public CancellationToken ApplicationStarted => CancellationToken.None;

        public CancellationToken ApplicationStopping { get; }

        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication()
        {
        }
    }
}
