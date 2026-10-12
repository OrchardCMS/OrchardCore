using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;

namespace OrchardCore.Environment.Options;

/// <summary>
/// Provides an Orchard Core distributed signal-backed <see cref="IOptionsChangeTokenSource{TOptions}"/>
/// so the default <see cref="IOptionsMonitor{TOptions}"/> can refresh options after an update notification.
/// </summary>
/// <remarks>
/// The change token is triggered either by the key of this options type and name, or by
/// <see cref="OptionsUpdateSignal.AllKey"/> that invalidates every signal-backed options instance.
/// </remarks>
/// <typeparam name="TOptions">The options type to observe.</typeparam>
public sealed class SignalOptionsChangeTokenSource<TOptions> : IOptionsChangeTokenSource<TOptions>
    where TOptions : class
{
    private readonly ISignal _signal;
    private readonly string _key;

    /// <summary>
    /// Creates a change-token source for the default options name.
    /// </summary>
    /// <param name="signal">The Orchard Core signal service.</param>
    public SignalOptionsChangeTokenSource(ISignal signal)
        : this(signal, Microsoft.Extensions.Options.Options.DefaultName)
    {
    }

    /// <summary>
    /// Creates a change-token source for the specified named options instance.
    /// </summary>
    /// <param name="signal">The Orchard Core signal service.</param>
    /// <param name="name">The named options instance to observe.</param>
    public SignalOptionsChangeTokenSource(ISignal signal, string name)
    {
        _signal = signal;
        Name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
        _key = OptionsUpdateSignal.GetKey(typeof(TOptions), Name);
    }

    /// <summary>
    /// Gets the named options instance observed by this token source.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc />
    public IChangeToken GetChangeToken()
        => new CompositeChangeToken(
        [
            _signal.GetToken(_key),
            _signal.GetToken(OptionsUpdateSignal.AllKey),
        ]);
}
