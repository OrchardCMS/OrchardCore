using System;
using Microsoft.Extensions.Options;

namespace OrchardCore.Benchmarks.Support;

internal sealed class FakeOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
{
    public FakeOptionsMonitor(TOptions currentValue)
    {
        CurrentValue = currentValue;
    }

    public TOptions CurrentValue { get; }

    public TOptions Get(string name) => CurrentValue;

    public IDisposable OnChange(Action<TOptions, string> listener) => null;
}
