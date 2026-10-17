using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class AdminListHelper
{
    /// <summary>
    /// Saves the site's admin list defaults through the settings editor.
    /// </summary>
    /// <param name="page">The authenticated admin page.</param>
    /// <param name="layout">The default list layout.</param>
    /// <param name="allowUserSelection">Whether per-list browser preferences are enabled.</param>
    /// <param name="actionsLayout">The default row actions layout.</param>
    public static async Task SetAdminListSettingsAsync(this IPage page, string layout, bool allowUserSelection, string actionsLayout = "Buttons")
    {
        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        await page.GetByLabel("List layout", new() { Exact = true }).SelectOptionAsync(layout);
        await page.GetByLabel("List actions layout", new() { Exact = true }).SelectOptionAsync(actionsLayout);
        await page.GetByLabel("Let users choose the layout of a list").SetCheckedAsync(allowUserSelection);
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
