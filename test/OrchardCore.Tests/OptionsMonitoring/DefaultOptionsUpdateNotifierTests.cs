using OrchardCore.Data.Documents;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Distributed;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Tests.OptionsMonitoring;

public class DefaultOptionsUpdateNotifierTests
{
    [Fact]
    public async Task RequestUpdate_SeveralTypes_SignalsOncePerTypeInRequestOrder()
    {
        var harness = new NotifierHarness();

        await harness.RunAsync(notifier =>
        {
            notifier
                .RequestUpdate<ThirdOptions>()
                .RequestUpdate<FirstOptions>()
                .RequestUpdate<ThirdOptions>()
                .RequestUpdate<SecondOptions>()
                .RequestUpdate<FirstOptions>();
        });

        Assert.Equal(
        [
            OptionsUpdateSignal.GetKey(typeof(ThirdOptions), string.Empty),
            OptionsUpdateSignal.GetKey(typeof(FirstOptions), string.Empty),
            OptionsUpdateSignal.GetKey(typeof(SecondOptions), string.Empty),
        ], harness.SignaledKeys);
    }

    [Fact]
    public async Task RequestUpdate_SameTypeDifferentNames_SignalsEachName()
    {
        var harness = new NotifierHarness();

        await harness.RunAsync(notifier =>
        {
            notifier
                .RequestUpdate<FirstOptions>("B")
                .RequestUpdate<FirstOptions>("A")
                .RequestUpdate<FirstOptions>("B");
        });

        Assert.Equal(
        [
            OptionsUpdateSignal.GetKey(typeof(FirstOptions), "B"),
            OptionsUpdateSignal.GetKey(typeof(FirstOptions), "A"),
        ], harness.SignaledKeys);
    }

    [Fact]
    public async Task RequestUpdate_CommitNotSucceeded_DoesNotSignal()
    {
        var harness = new NotifierHarness { CommitSucceeds = false };

        await harness.RunAsync(notifier => notifier.RequestUpdate<FirstOptions>());

        Assert.Empty(harness.SignaledKeys);
        Assert.Empty(harness.UpdatedTenants);
    }

    [Fact]
    public async Task RequestUpdate_CommitSucceeded_NotifiesOtherInstancesOnce()
    {
        var harness = new NotifierHarness();

        await harness.RunAsync(notifier =>
        {
            notifier
                .RequestUpdate<FirstOptions>()
                .RequestUpdate<SecondOptions>();
        });

        Assert.Equal([ShellSettings.DefaultShellName], harness.UpdatedTenants);
    }

    [Fact]
    public async Task RequestUpdateAll_CommitSucceeded_SignalsRequestedOptionsThenAllOptions()
    {
        var harness = new NotifierHarness();

        await harness.RunAsync(notifier =>
        {
            notifier
                .RequestUpdateAll()
                .RequestUpdate<FirstOptions>();
        });

        Assert.Equal(
        [
            OptionsUpdateSignal.GetKey(typeof(FirstOptions), string.Empty),
            .. Enumerable.Repeat(OptionsUpdateSignal.AllKey, OptionsUpdateSignal.AllKeySignalCount),
        ], harness.SignaledKeys);

        Assert.Equal([ShellSettings.DefaultShellName], harness.UpdatedTenants);
    }

    [Fact]
    public async Task RequestUpdateAll_CommitNotSucceeded_DoesNotSignal()
    {
        var harness = new NotifierHarness { CommitSucceeds = false };

        await harness.RunAsync(notifier => notifier.RequestUpdateAll());

        Assert.Empty(harness.SignaledKeys);
    }

    private sealed class NotifierHarness
    {
        public bool CommitSucceeds { get; set; } = true;

        public List<string> SignaledKeys { get; } = [];

        public List<string> UpdatedTenants { get; } = [];

        public async Task RunAsync(Action<IOptionsUpdateNotifier> requestUpdates)
        {
            var afterCommitCallbacks = new List<DocumentStoreCommitSuccessDelegate>();

            var documentStore = new Mock<IDocumentStore>();
            documentStore
                .Setup(store => store.AfterCommitSuccess<DefaultOptionsUpdateNotifier>(It.IsAny<DocumentStoreCommitSuccessDelegate>()))
                .Callback<DocumentStoreCommitSuccessDelegate>(afterCommitCallbacks.Add);

            var signal = new Mock<ISignal>();
            signal
                .Setup(signal => signal.SignalTokenAsync(It.IsAny<string>()))
                .Callback<string>(SignaledKeys.Add)
                .Returns(Task.CompletedTask);

            var shellHost = new Mock<IShellHost>();
            shellHost
                .Setup(host => host.GetScopeAsync(It.IsAny<ShellSettings>()))
                .ThrowsAsync(new InvalidOperationException());

            var optionsEvents = new ShellOptionsEvents
            {
                UpdatedAsync = tenant =>
                {
                    UpdatedTenants.Add(tenant);
                    return Task.CompletedTask;
                },
            };

            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton(documentStore.Object)
                .AddSingleton(signal.Object)
                .AddSingleton(shellHost.Object)
                .AddSingleton(optionsEvents)
                .AddScoped<IOptionsUpdateNotifier, DefaultOptionsUpdateNotifier>();

            var shellContext = new ShellContext
            {
                Settings = new ShellSettings().AsDefaultShell().AsRunning(),
                ServiceProvider = services.BuildServiceProvider(),
            };

            // The deferred tasks queued by the notifier run when this scope ends.
            await new ShellScope(shellContext).UsingAsync(async scope =>
            {
                var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();
                requestUpdates(notifier);

                if (CommitSucceeds)
                {
                    foreach (var callback in afterCommitCallbacks.ToArray())
                    {
                        await callback();
                    }
                }
            }, activateShell: false);
        }
    }

    private sealed class FirstOptions;

    private sealed class SecondOptions;

    private sealed class ThirdOptions;
}
