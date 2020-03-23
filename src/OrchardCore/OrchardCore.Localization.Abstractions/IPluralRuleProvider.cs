using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Contract to provide a pluralization rule for cultures.
    /// </summary>
    public interface IPluralRuleProvider
    {
        /// <summary>
        /// Gets the order for <see cref="IPluralRuleProvider"/> implementation to be executed.
        /// </summary>
        /// <remarks>Set the <see cref="Order"/> to a negative value in order to get called before the default implementation, or a
        /// positive value to be used as a fallback one.</remarks>
        int Order { get; }

        /// <summary>
        /// Gets a pluralization rule for the specified culture if there is one.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="rule">The <see cref="PluralizationRuleDelegate"/>.</param>
        /// <returns>A boolean value indicates whether the culture rule is retrieved?</returns>
        bool TryGetRule(CultureInfo culture, out PluralizationRuleDelegate rule);
    }
}
