using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class ButtonHelper
{
    public static Task ClickCreateAsync(this IPage page)
        => page.Locator(".btn.create").ClickAsync();

    public static Task ClickSaveAsync(this IPage page)
        => page.Locator(".btn.save").ClickAsync();

    public static Task ClickSaveContinueAsync(this IPage page)
        => page.Locator(".dropdown-item.save-continue").ClickAsync();

    public static Task ClickCancelAsync(this IPage page)
        => page.Locator(".btn.cancel").ClickAsync();

    public static Task ClickPublishAsync(this IPage page)
        => page.Locator(".btn.publish").ClickAsync();

    public static Task ClickPublishContinueAsync(this IPage page)
        => page.Locator(".dropdown-item.publish-continue").ClickAsync();

    public static Task ClickModalOkAsync(this IPage page)
        => page.Locator("#modalOkButton").ClickAsync();
}
