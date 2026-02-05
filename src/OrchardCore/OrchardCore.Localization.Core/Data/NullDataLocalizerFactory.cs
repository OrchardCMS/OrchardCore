namespace OrchardCore.Localization.Data;

/// <summary>
/// A factory that creates instances of <see cref="NullDataLocalizer"/>.
/// This is used when the OrchardCore.DataLocalization feature is not enabled.
/// </summary>
internal sealed class NullDataLocalizerFactory : IDataLocalizerFactory
{
    private static readonly NullDataLocalizer _instance = new();

    /// <inheritdoc/>
    public IDataLocalizer Create() => _instance;
}
