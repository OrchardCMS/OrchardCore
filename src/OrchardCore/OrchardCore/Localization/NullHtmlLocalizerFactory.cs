using Microsoft.AspNetCore.Mvc.Localization;

namespace OrchardCore.Localization;

/// Represents a null <see cref="IHtmlLocalizerFactory"/> which is used by default when the localization module is disabled.
/// <remarks>
/// LocalizedHtmlString's arguments will be HTML encoded and not the main string. So the result
/// should just contain the localized string containing the formatting placeholders {0...} as is.
/// </remarks>
public class NullHtmlLocalizerFactory : IHtmlLocalizerFactory
{
    /// <inheritdocs />
    public IHtmlLocalizer Create(string baseName, string location) => NullHtmlLocalizer.Instance;

    /// <inheritdocs />
    public IHtmlLocalizer Create(Type resourceSource) => NullHtmlLocalizer.Instance;
}
