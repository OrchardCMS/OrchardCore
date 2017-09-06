using System.Globalization;

namespace OrchardCore.Localization
{
    public delegate int PluralizationRuleDelegate(int count);

    /// <summary>
    /// An implementation of this interface is able to resolve pluralization rules for a specific culture.
    /// Set the Order to a negative value in order to get called before the default implementation, or a 
    /// positive value to be used as a fallback one.
    /// </summary>
    public interface IPluralRuleProvider
    {
        int Order { get; }

        bool TryGetRule(CultureInfo culture, out PluralizationRuleDelegate rule);
    }
}