using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class ButtonHelper
{
    public static Task ClickCreateAsync(this IPage page)
        => page.Locator(".btn.create").ClickAsync();

    public static Task ClickSaveAsync(this IPage page)
        => ClickActionAsync(page, ".btn.save", alternative: false);

    public static Task ClickSaveContinueAsync(this IPage page)
        => ClickActionAsync(page, ".btn.save", alternative: true);

    public static Task ClickCancelAsync(this IPage page)
        => page.Locator(".btn.cancel").ClickAsync();

    public static Task ClickPublishAsync(this IPage page)
        => ClickActionAsync(page, ".btn.publish", alternative: false);

    public static Task ClickPublishContinueAsync(this IPage page)
        => ClickActionAsync(page, ".btn.publish", alternative: true);

    // A split button with a [data-action-group] remembers the last used action: its main button then submits the
    // alternative action, and its dropdown item the default one. Clicks the button submitting the requested action.
    private static async Task ClickActionAsync(IPage page, string mainButtonSelector, bool alternative)
    {
        var mainButton = page.Locator(mainButtonSelector);
        var group = page.Locator(".btn-group[data-action-group]").Filter(new() { Has = mainButton });

        if (await group.CountAsync() == 0)
        {
            await mainButton.ClickAsync();

            return;
        }

        var action = await group.GetAttributeAsync(alternative ? "data-action-alt" : "data-action-default");

        if (await mainButton.GetAttributeAsync("value") == action)
        {
            await mainButton.ClickAsync();

            return;
        }

        await group.Locator(".dropdown-toggle").ClickAsync();
        await group.Locator($".dropdown-item[value='{action}']").ClickAsync();
    }

    public static Task ClickModalOkAsync(this IPage page)
        => page.Locator("#modalOkButton").ClickAsync();
}
