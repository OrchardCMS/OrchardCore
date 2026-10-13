namespace OrchardCore.Tests.Stubs;

/// <summary>
/// An <see cref="IOptionsMonitor{TOptions}"/> that always returns the same options instance.
/// Intended for unit testing.
/// </summary>
public sealed class StaticOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
{
    public StaticOptionsMonitor(TOptions currentValue)
    {
        CurrentValue = currentValue;
    }

    public TOptions CurrentValue { get; set; }

    public TOptions Get(string name) => CurrentValue;

    public IDisposable OnChange(Action<TOptions, string> listener) => null;
}
