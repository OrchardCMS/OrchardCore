using System;
using System.Globalization;

namespace Orchard.Localization.Abstractions
{

    public delegate int PluralRuleDelegate(int count);


    public interface IPluralRuleProvider
    {
        int Priority { get; }

        PluralRuleDelegate GetRule(CultureInfo culture);
    }
}