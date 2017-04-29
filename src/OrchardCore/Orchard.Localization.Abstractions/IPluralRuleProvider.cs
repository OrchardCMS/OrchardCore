using System;
using System.Globalization;

namespace Orchard.Localization.Abstractions
{
    public interface IPluralRuleProvider
    {
        Func<int, int> GetRule(CultureInfo culture);
    }
}