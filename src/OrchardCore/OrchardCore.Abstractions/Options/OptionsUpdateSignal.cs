using OrchardCore.Environment.Cache;

namespace OrchardCore.Environment.Options;

/// <summary>
/// Builds the distributed signal key used to invalidate a specific options type and name.
/// </summary>
public static class OptionsUpdateSignal
{
    private const string Prefix = "OrchardCore.Environment.Options.Update:";

    /// <summary>
    /// The distributed signal key that invalidates every options instance observed through a
    /// <see cref="SignalOptionsChangeTokenSource{TOptions}"/>, whatever its type and name.
    /// </summary>
    public const string AllKey = Prefix + "*";

    /// <summary>
    /// The number of times <see cref="AllKey"/> is signaled by <see cref="SignalAllAsync(ISignal)"/>.
    /// </summary>
    /// <remarks>
    /// An <see cref="Microsoft.Extensions.Options.IOptionsMonitor{TOptions}"/> rebuilds an options instance as soon
    /// as it is invalidated. When all the options are invalidated at once, an options instance built from other
    /// options may be rebuilt before them, so from their previous values. Each additional invalidation rebuilds one
    /// more level of options built from up to date options, e.g. settings, then the options of an authentication
    /// handler built from them, then the authentication options built from the handler options.
    /// </remarks>
    public const int AllKeySignalCount = 4;

    /// <summary>
    /// Invalidates every signal-backed options instance, whatever its type and name, including the options
    /// built from other signal-backed options.
    /// </summary>
    /// <param name="signal">The Orchard Core signal service.</param>
    public static async Task SignalAllAsync(ISignal signal)
    {
        ArgumentNullException.ThrowIfNull(signal);

        for (var i = 0; i < AllKeySignalCount; i++)
        {
            await signal.SignalTokenAsync(AllKey);
        }
    }

    /// <summary>
    /// Gets the distributed signal key for the specified options type and name.
    /// </summary>
    /// <param name="optionsType">The options type to invalidate.</param>
    /// <param name="name">The named options instance to invalidate.</param>
    public static string GetKey(Type optionsType, string name)
    {
        ArgumentNullException.ThrowIfNull(optionsType);

        name ??= Microsoft.Extensions.Options.Options.DefaultName;

        var typeName = optionsType.AssemblyQualifiedName ?? optionsType.FullName ?? optionsType.Name;

        return $"{Prefix}{typeName.Length}:{typeName}:{name.Length}:{name}";
    }
}
