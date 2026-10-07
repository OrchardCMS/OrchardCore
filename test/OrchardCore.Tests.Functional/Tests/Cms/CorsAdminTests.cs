using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Cors' policy list/editor (cors-admin-index.ts), rewritten from a
// jQuery Vue 2 app to native DOM + Vue 3 across #19489/#19774. Unlike the AJAX-injected
// widgets covered elsewhere in this session, this is a single full-page Vue app (no
// AJAX-injection double-init risk) whose "save" action is a real form submit+reload
// (Vue.createApp(...).save() sets a hidden textarea's value to JSON.stringify(policies)
// and calls corsForm.submit()) - so this test's real regression-catching value is that the
// full add -> edit -> persist -> reload round-trip through that JSON-serialize/submit path
// actually works, not just that the Vue app renders.
public sealed class CorsAdminTests : CmsTestBase<CorsAdminTestsFixture>, IClassFixture<CorsAdminTestsFixture>
{
    public CorsAdminTests(CorsAdminTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AddCorsPolicy_OriginsAndMethodOption_Persist()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Cors");

        var corsAdmin = page.Locator("#corsAdmin");
        await Assertions.Expect(corsAdmin.Locator("button.btn-secondary")).ToHaveCountAsync(1);

        await corsAdmin.Locator("button.btn-secondary").ClickAsync(); // "Add Policy".

        // The policy-details card's first plain text input is the policy name field
        // (policyDetailsComponent's template: <h3>{{ policy.name }}</h3> then a single
        // "ocat-wrapper" text input for policy.name before any checkboxes/options-lists).
        var policyNameInput = corsAdmin.Locator("input[type='text'].form-control").First;
        await policyNameInput.FillAsync("");
        await policyNameInput.FillAsync("FunctionalTestPolicy");

        // Allowed Origins is the first options-list (allowAnyOrigin unchecked by default,
        // per newPolicy()'s initial state) - add one origin via its own text input + Add
        // button (the shared optionsListComponent template: one input.form-control + one
        // "Add {optionType}" button per options-list instance).
        var originsInput = corsAdmin.Locator("#allowed-origins").Locator("xpath=ancestor::div[contains(@class,'card')][1]")
            .Locator("input[type='text'].form-control");
        await originsInput.FillAsync("https://example.test");
        await corsAdmin.Locator("#allowed-origins").Locator("xpath=ancestor::div[contains(@class,'card')][1]")
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { NameRegex = new System.Text.RegularExpressions.Regex("Add") })
            .ClickAsync();

        // Turn off "allow any method" so the allowed-methods options-list becomes
        // meaningful, then add one explicit method.
        var allowAnyMethod = corsAdmin.Locator("#allowed-methods");
        if (await allowAnyMethod.IsCheckedAsync())
        {
            await allowAnyMethod.UncheckAsync();
        }

        var methodsInput = corsAdmin.Locator("#allowed-methods").Locator("xpath=ancestor::div[contains(@class,'card')][1]")
            .Locator("input[type='text'].form-control");
        await methodsInput.FillAsync("GET");
        await corsAdmin.Locator("#allowed-methods").Locator("xpath=ancestor::div[contains(@class,'card')][1]")
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { NameRegex = new System.Text.RegularExpressions.Regex("Add") })
            .ClickAsync();

        // Save submits the real <form id="corsForm"> (hidden textarea set to
        // JSON.stringify(policies)) - a genuine full-page reload, not AJAX.
        await corsAdmin.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync("/Admin/Cors");
        await Assertions.Expect(page.Locator("text=FunctionalTestPolicy")).ToBeVisibleAsync();

        // Re-open and confirm the origin/method actually persisted through the JSON
        // round-trip, not just the policy name.
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "FunctionalTestPolicy" })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Edit" })
            .ClickAsync();

        var reopenedCorsAdmin = page.Locator("#corsAdmin");
        await Assertions.Expect(reopenedCorsAdmin.Locator("text=https://example.test")).ToBeVisibleAsync();
        await Assertions.Expect(reopenedCorsAdmin.GetByText("GET", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(reopenedCorsAdmin.Locator("#allowed-methods")).Not.ToBeCheckedAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
