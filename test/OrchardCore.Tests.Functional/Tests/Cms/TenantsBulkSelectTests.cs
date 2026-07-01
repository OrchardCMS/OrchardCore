using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the vanilla-JS bulk-select toolbar on the Tenants admin list
// (src/OrchardCore.Modules/OrchardCore.Tenants/Views/Admin/Index.cshtml) -
// SaasFixture always provisions at least two tenants (the default tenant
// plus its own generated Tenant), enough to exercise "select all" and the
// selected-count/actions-dropdown toggling without needing to actually run
// a bulk action against them.
public sealed class TenantsBulkSelectTests : IClassFixture<SaasFixture>
{
    private readonly SaasFixture _fixture;

    public TenantsBulkSelectTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TenantsBulkSelect_SelectAll_TogglesActionsAndCount()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin/Tenants");

        var actions = page.Locator("#actions");
        var items = page.Locator("#items");
        var selectedItems = page.Locator("#selected-items");
        var checkboxes = page.Locator("input[type='checkbox'][name='tenantNames']");

        var count = await checkboxes.CountAsync();
        Assert.True(count >= 2, "Expected at least 2 tenants (default + SaasFixture's own) to select from.");

        await Assertions.Expect(actions).ToBeHiddenAsync();
        await Assertions.Expect(items).ToBeVisibleAsync();

        await page.Locator("#select-all").ClickAsync();

        await Assertions.Expect(actions).ToBeVisibleAsync();
        await Assertions.Expect(items).ToBeHiddenAsync();
        // The label text itself is localized ("2 sélectionné(s)" etc.), so
        // assert on the actual checked-state and count instead of the text.
        Assert.Contains(count.ToString(), await selectedItems.InnerTextAsync());
        Assert.Equal(count, await page.Locator("input[type='checkbox'][name='tenantNames']:checked").CountAsync());

        // Unchecking every box individually (rather than a second click on
        // select-all, which the page's own logic doesn't wire back to
        // clearing every checkbox) restores the default view.
        for (var i = 0; i < count; i++)
        {
            await checkboxes.Nth(i).UncheckAsync();
        }

        await Assertions.Expect(actions).ToBeHiddenAsync();
        await Assertions.Expect(items).ToBeVisibleAsync();

        await page.CloseAsync();
    }
}
