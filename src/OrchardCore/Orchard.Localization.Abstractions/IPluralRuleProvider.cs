using System;

namespace Orchard.Localization.Abstractions
{
    public interface IPluralRuleProvider
    {
        Func<int, int> GetRule(string cultureName);
    }
}