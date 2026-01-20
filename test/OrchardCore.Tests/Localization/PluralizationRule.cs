using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class PluralizationRule
{
    public static readonly PluralizationRuleDelegate Czech = n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);

    public static readonly PluralizationRuleDelegate English = n => (n == 1) ? 0 : 1;

    public static readonly PluralizationRuleDelegate Arabic = n => (n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n % 100 >= 3 && n % 100 <= 10 ? 3 : n % 100 >= 11 ? 4 : 5);
}
