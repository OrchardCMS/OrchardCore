using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the split buttons of the edit pages' action bar, which remember the last used action.
public sealed class ActionBarTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    private const string EditRoleUrl = "/Admin/Roles/Edit/Editor";

    public ActionBarTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task SaveAndContinue_LastUsedAction_BecomesTheMainButton()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync(EditRoleUrl);

        var mainButton = page.Locator(".action-bar-bottom .btn-group[data-action-group] > button[type='submit']");
        await Assertions.Expect(mainButton).ToHaveTextAsync("Save");
        await Assertions.Expect(mainButton).ToHaveAttributeAsync("value", "Save");

        // "and continue" saves and stays on the editor, which then offers it as the main action.
        await page.ClickSaveContinueAsync();
        await page.WaitForURLAsync($"**{EditRoleUrl}");
        await Assertions.Expect(page.Locator(".message-success")).ToBeVisibleAsync();
        await Assertions.Expect(mainButton).ToHaveTextAsync("Save and continue");
        await Assertions.Expect(mainButton).ToHaveAttributeAsync("value", "SaveAndContinue");

        // The main button keeps saving and continuing.
        await mainButton.ClickAsync();
        await page.WaitForURLAsync($"**{EditRoleUrl}");
        await Assertions.Expect(mainButton).ToHaveTextAsync("Save and continue");

        // Saving from the dropdown returns to the list and restores the default action.
        await page.ClickSaveAsync();
        await page.WaitForURLAsync(url => !url.Contains("/Edit/", StringComparison.Ordinal));

        await page.GotoAndAssertOkAsync(EditRoleUrl);
        await Assertions.Expect(mainButton).ToHaveTextAsync("Save");
        await Assertions.Expect(mainButton).ToHaveAttributeAsync("value", "Save");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ContentEditor_Actions_AreOrderedFromCancelToPublish()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/Article/Create?returnUrl=%2FAdmin%2FContents%2FContentItems");

        // The visible controls of the bar, a split button counting as its main button.
        var labels = await page.Locator(
            ".action-bar-bottom > a:visible, .action-bar-bottom > button:visible, .action-bar-bottom > .btn-group:visible > button[type='submit']")
            .AllInnerTextsAsync();

        // Cancel first, then the secondary actions, and the main action last.
        Assert.Equal(["Cancel", "Preview", "Save Draft", "Publish"], labels.Select(label => label.Trim()));

        await page.CloseAsync();
    }
}
