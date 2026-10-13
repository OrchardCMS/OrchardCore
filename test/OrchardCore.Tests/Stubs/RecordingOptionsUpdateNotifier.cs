using OrchardCore.Environment.Options;

namespace OrchardCore.Tests.Stubs;

/// <summary>
/// An <see cref="IOptionsUpdateNotifier"/> that records the requested updates in order.
/// Intended for unit testing.
/// </summary>
public sealed class RecordingOptionsUpdateNotifier : IOptionsUpdateNotifier
{
    public List<(Type OptionsType, string Name)> Requests { get; } = [];

    public IOptionsUpdateNotifier RequestUpdate<TOptions>(string name = "") where TOptions : class
    {
        Requests.Add((typeof(TOptions), name ?? string.Empty));

        return this;
    }

    public bool UpdateAllRequested { get; private set; }

    public IOptionsUpdateNotifier RequestUpdateAll()
    {
        UpdateAllRequested = true;

        return this;
    }
}
