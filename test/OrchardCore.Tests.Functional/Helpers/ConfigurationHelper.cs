using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class ConfigurationHelper
{
    public static async Task SetPageSizeAsync(IPage page, string prefix, string size)
    {
        await page.GotoAsync($"{prefix}/Admin/Settings/general");
        await page.Locator("#ISite_PageSize").ClearAsync();
        await page.Locator("#ISite_PageSize").FillAsync(size);
        await ButtonHelper.ClickSaveAsync(page);
        await page.Locator(".message-success").WaitForAsync();
    }
}
