using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class ButtonHelper
{
    public static Task ClickCreateAsync(IPage page)
        => page.Locator(".btn.create").ClickAsync();

    public static Task ClickSaveAsync(IPage page)
        => page.Locator(".btn.save").ClickAsync();

    public static Task ClickSaveContinueAsync(IPage page)
        => page.Locator(".dropdown-item.save-continue").ClickAsync();

    public static Task ClickCancelAsync(IPage page)
        => page.Locator(".btn.cancel").ClickAsync();

    public static Task ClickPublishAsync(IPage page)
        => page.Locator(".btn.public").ClickAsync();

    public static Task ClickPublishContinueAsync(IPage page)
        => page.Locator(".dropdown-item.publish-continue").ClickAsync();

    public static Task ClickModalOkAsync(IPage page)
        => page.Locator("#modalOkButton").ClickAsync();
}
