using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization;

/// <summary>
/// Contract that extends <see cref="IStringLocalizer"/> to support pluralization.
/// </summary>
public interface IPluralStringLocalizer : IStringLocalizer
{
    /// <summary>
    /// Gets the localized strings.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <param name="arguments">Optional parameters that can be used inside the resource key.</param>
    /// <returns>A list of localized strings including the plural forms.</returns>
    [Obsolete("This method is deprecated, please use GetTranslationAsync instead.")]
    (LocalizedString, object[]) GetTranslation(string name, params object[] arguments)
        => GetTranslationAsync(name, arguments).GetAwaiter().GetResult();

    /// <summary>
    /// Gets the localized strings.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <param name="arguments">Optional parameters that can be used inside the resource key.</param>
    /// <returns>A list of localized strings including the plural forms.</returns>
    Task<(LocalizedString, object[])> GetTranslationAsync(string name, params object[] arguments);
}
