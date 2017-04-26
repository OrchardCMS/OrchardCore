using System;

namespace Orchard.Localization
{
    public interface IPluralRuleProvider
    {
        Func<int, int> GetRule(string cultureName);
    }
}