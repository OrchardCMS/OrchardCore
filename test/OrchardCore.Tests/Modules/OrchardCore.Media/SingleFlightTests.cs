#nullable enable

using OrchardCore.Media.Processing;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class SingleFlightTests
{
    [Fact]
    public async Task ScheduleAsync_CoalescesConcurrentCallsForSameKey()
    {
        var singleFlight = new SingleFlight<string, string>();
        var invocations = 0;
        var release = new TaskCompletionSource();

        async Task<string?> Factory(string key)
        {
            Interlocked.Increment(ref invocations);

            // Hold the worker open so the other callers join the same in-flight operation.
            await release.Task;

            return "value-for-" + key;
        }

        var callers = Enumerable
            .Range(0, 10)
            .Select(_ => singleFlight.ScheduleAsync("key", Factory))
            .ToArray();

        release.SetResult();

        var results = await Task.WhenAll(callers);

        Assert.Equal(1, invocations);
        Assert.All(results, r => Assert.Equal("value-for-key", r));
    }

    [Fact]
    public async Task ScheduleAsync_RunsFactoryAgainAfterCompletion()
    {
        var singleFlight = new SingleFlight<string, string>();
        var invocations = 0;

        Task<string?> Factory(string key)
        {
            Interlocked.Increment(ref invocations);

            return Task.FromResult<string?>(key);
        }

        await singleFlight.ScheduleAsync("key", Factory);
        await singleFlight.ScheduleAsync("key", Factory);

        // The key is removed once the worker completes, so a later call runs the factory again.
        Assert.Equal(2, invocations);
    }

    [Fact]
    public async Task ScheduleAsync_DifferentKeysRunIndependently()
    {
        var singleFlight = new SingleFlight<string, string>();
        var invocations = 0;

        Task<string?> Factory(string key)
        {
            Interlocked.Increment(ref invocations);

            return Task.FromResult<string?>(key);
        }

        var a = await singleFlight.ScheduleAsync("a", Factory);
        var b = await singleFlight.ScheduleAsync("b", Factory);

        Assert.Equal(2, invocations);
        Assert.Equal("a", a);
        Assert.Equal("b", b);
    }

    [Fact]
    public async Task ScheduleAsync_FailureIsNotCachedAndAllWaitersObserveException()
    {
        var singleFlight = new SingleFlight<string, string>();
        var invocations = 0;
        var release = new TaskCompletionSource();

        async Task<string?> ThrowingFactory(string key)
        {
            Interlocked.Increment(ref invocations);
            await release.Task;

            throw new InvalidOperationException("boom");
        }

        var callers = Enumerable
            .Range(0, 5)
            .Select(_ => singleFlight.ScheduleAsync("key", ThrowingFactory))
            .ToArray();

        release.SetResult();

        foreach (var caller in callers)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => caller);
        }

        // Only one worker ran for the burst, and the failed key was removed so a retry runs again.
        Assert.Equal(1, invocations);

        var retry = await singleFlight.ScheduleAsync("key", _ => Task.FromResult<string?>("ok"));

        Assert.Equal("ok", retry);
    }
}
