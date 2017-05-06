using System;
using System.Globalization;

namespace Orchard.Localization.Abstractions
{
    public delegate int PluralizationRuleDelegate(int count);

    public interface IPluralRuleProvider
    {
        int Order { get; }

        PluralizationRuleDelegate GetRule(CultureInfo culture);
    }
}