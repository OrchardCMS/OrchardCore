using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
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
        (LocalizedString, object[]) GetTranslation(string name, params object[] arguments);
    }
}
