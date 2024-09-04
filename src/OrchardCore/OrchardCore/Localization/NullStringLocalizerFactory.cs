using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization;

/// Represents a null <see cref="IStringLocalizerFactory"/> which is used by default when the localization module is disabled.
/// <remarks>
/// A LocalizedString is not encoded, so it can contain the formatted string
/// including the argument values.
/// </remarks>
public class NullStringLocalizerFactory : IStringLocalizerFactory
{
    /// <inheritdocs />
    public IStringLocalizer Create(Type resourceSource) => NullStringLocalizer.Instance;

    /// <inheritdocs />
    public IStringLocalizer Create(string baseName, string location) => NullStringLocalizer.Instance;
}
