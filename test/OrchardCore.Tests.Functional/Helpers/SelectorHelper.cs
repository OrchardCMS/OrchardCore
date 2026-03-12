using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class SelectorHelper
{
    public static ILocator GetByCy(IPage page, string selector, bool exact = false)
    {
        return exact
            ? page.Locator($"[data-cy=\"{selector}\"]")
            : page.Locator($"[data-cy^=\"{selector}\"]");
    }

    public static ILocator FindByCy(ILocator locator, string selector, bool exact = false)
    {
        return exact
            ? locator.Locator($"[data-cy=\"{selector}\"]")
            : locator.Locator($"[data-cy^=\"{selector}\"]");
    }
}
