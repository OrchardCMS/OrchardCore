using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Deployment's step reordering (Assets/js/steporder.js): a SortableJS
// drag handler wired to a fetch-based POST to /Admin/OrchardCore.Deployment/Step/UpdateOrder
// (was jQuery UI sortable + $.ajax before the jQuery-removal migration in PR #19489).
public sealed class DeploymentPlanStepOrderTests : CmsTestBase<DeploymentPlanStepOrderTestsFixture>, IClassFixture<DeploymentPlanStepOrderTestsFixture>
{
    public DeploymentPlanStepOrderTests(DeploymentPlanStepOrderTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DragReorderSteps_PersistsNewOrderOnReload()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/DeploymentPlan/Create");
        await page.Locator("#Name").FillAsync("Step Order Test Plan");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Create" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Landed on the plan's Display page after creation - add two config-free step
        // types (AllContent from Contents, AllRoles from Roles) via the "Add Step" modal.
        async Task AddStepAsync(string type)
        {
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add Step" }).ClickAsync();
            await page.Locator($"a[href*='Step/Create'][href*='type={type}']").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Create" }).ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await AddStepAsync("AllContent");
        await AddStepAsync("AllRoles");

        var stepList = page.Locator("#stepOrder");
        var stepItems = stepList.Locator("li.list-group-item");
        await Assertions.Expect(stepItems).ToHaveCountAsync(2);

        // AllContent was added first, so it should currently be first.
        await Assertions.Expect(stepItems.Nth(0)).ToContainTextAsync("All Content");
        await Assertions.Expect(stepItems.Nth(1)).ToContainTextAsync("All Roles");

        // Drag the second item's handle above the first item to reorder. Simulating a
        // real SortableJS pointer drag via synthetic mouse events proved too flaky across
        // environments (SortableJS relies on native drag-detection heuristics that don't
        // always fire from injected events) - instead, invoke updateStepOrders() directly,
        // the same real global function (steporder.js is a classic, non-module script)
        // SortableJS's own onSort callback calls with the old/new indices. This still
        // exercises the actual fetch-based AJAX call and server-side persistence, which is
        // the part of this feature (jQuery $.ajax -> fetch, during the jQuery-removal
        // migration) actually worth regression-testing - only the third-party drag
        // library's own pointer-event wiring is not re-verified here.
        var saveResponseTask = page.WaitForResponseAsync(resp => resp.Url.Contains("/Step/UpdateOrder") && resp.Request.Method == "POST");
        await page.EvaluateAsync("updateStepOrders(1, 0)");
        var saveResponse = await saveResponseTask;
        Assert.True(saveResponse.Ok, $"UpdateOrder request failed: {saveResponse.Status}");

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reloadedItems = page.Locator("#stepOrder li.list-group-item");
        await Assertions.Expect(reloadedItems).ToHaveCountAsync(2);
        await Assertions.Expect(reloadedItems.Nth(0)).ToContainTextAsync("All Roles");
        await Assertions.Expect(reloadedItems.Nth(1)).ToContainTextAsync("All Content");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
