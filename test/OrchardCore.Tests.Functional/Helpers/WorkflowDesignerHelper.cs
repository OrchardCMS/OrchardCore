using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace OrchardCore.Tests.Functional.Helpers;

/// <summary>
/// Drives the OrchardCore.Workflows designer through its <c>data-cy</c> hooks.
/// </summary>
public static class WorkflowDesignerHelper
{
    public static ILocator Designer(this IPage page) => page.Locator("[data-cy=workflow-designer]");

    public static ILocator CanvasSurface(this IPage page) => page.Locator("[data-cy=canvas-surface]");

    public static ILocator Activities(this IPage page) => page.Locator(".wfd-node");

    public static ILocator Activity(this IPage page, string activityId) => page.Locator($"[data-cy='activity-{activityId}']");

    public static ILocator Port(this IPage page, string activityId, string outcome) => page.Locator($"[data-cy='port-{activityId}-{outcome}']");

    public static ILocator Edges(this IPage page) => page.Locator("[data-cy^='edge-']");

    public static ILocator Edge(this IPage page, string sourceId, string outcome, string destinationId)
        => page.Locator($"[data-cy='edge-{sourceId}:{outcome}:{destinationId}']");

    public static ILocator ActivityForm(this IPage page) => page.Locator("[data-cy=panel-activity-form]");

    /// <summary>
    /// Creates a workflow type from its properties page, which then opens the designer. Returns its document id.
    /// </summary>
    public static async Task<long> CreateWorkflowTypeAsync(this IPage page, string name)
    {
        await page.GotoAndAssertOkAsync("/Admin/Workflows/Types/EditProperties");
        await page.Locator("#Name").FillAsync(name);
        await page.Locator("button.save").ClickAsync();
        await page.WaitForURLAsync("**/Admin/Workflows/Types/Edit/**");
        await page.WaitForDesignerAsync();

        return ParseWorkflowTypeId(page.Url);
    }

    /// <summary>
    /// Finds a workflow type in the list and returns its document id.
    /// </summary>
    public static async Task<long> FindWorkflowTypeIdAsync(this IPage page, string name)
    {
        // Searching by name, since the workflow types the tests create push the seeded ones off the first page.
        await page.GotoAndAssertOkAsync("/Admin/Workflows/Types?options.Search=" + Uri.EscapeDataString(name));
        var href = await page.Locator("a[href*='/Admin/Workflows/Types/Edit/']", new() { HasTextString = name }).First.GetAttributeAsync("href");

        return ParseWorkflowTypeId(href);
    }

    public static async Task OpenDesignerAsync(this IPage page, long workflowTypeId, string query = "")
    {
        await page.GotoAndAssertOkAsync($"/Admin/Workflows/Types/Edit/{workflowTypeId}{query}");
        await page.WaitForDesignerAsync();
    }

    public static async Task WaitForDesignerAsync(this IPage page)
    {
        await Assertions.Expect(page.CanvasSurface()).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-cy=designer-canvas]")).ToHaveAttributeAsync("aria-busy", "false");
    }

    public static Task WaitForSavedAsync(this IPage page)
        => Assertions.Expect(page.Locator("[data-cy=save-status]")).ToHaveAttributeAsync("data-status", "saved", new() { Timeout = 15_000 });

    /// <summary>
    /// Drags an activity from the toolbox onto the canvas, at (x, y) from the top-left of the canvas, and returns
    /// the id of the new activity. <paramref name="search"/> is typed in the toolbox search, so its category shows.
    /// </summary>
    public static async Task<string> AddActivityAsync(this IPage page, string activityName, string search, int x, int y)
    {
        var count = await page.Activities().CountAsync();

        await page.Locator("[data-cy=toolbox-search]").FillAsync(search);
        await page.Locator($"[data-cy='toolbox-activity-{activityName}']").DragToAsync(page.CanvasSurface(), new()
        {
            TargetPosition = new() { X = x, Y = y },
        });
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(count + 1);
        await page.Locator("[data-cy=toolbox-search]").FillAsync(string.Empty);

        // The added activity is the selected one.
        var activityId = await page.Locator(".wfd-node.is-selected").GetAttributeAsync("data-node-id");
        Assert.False(string.IsNullOrEmpty(activityId));

        return activityId;
    }

    /// <summary>
    /// Connects an outcome to an activity by dragging from the outcome's port onto the activity.
    /// </summary>
    public static async Task ConnectAsync(this IPage page, string sourceId, string outcome, string destinationId)
    {
        await page.DragAsync(page.Port(sourceId, outcome), page.Activity(destinationId).Locator(".wfd-node-header"));
        await Assertions.Expect(page.Edge(sourceId, outcome, destinationId)).ToHaveCountAsync(1);
    }

    /// <summary>
    /// Opens the editor of an activity in the activity panel (double-click), and waits for it to load.
    /// </summary>
    public static async Task EditActivityAsync(this IPage page, string activityId)
    {
        await page.Activity(activityId).Locator(".wfd-node-header").DblClickAsync();
        await Assertions.Expect(page.Activity(activityId)).ToHaveClassAsync(new Regex("is-selected"));

        // The server-rendered editor names the activity it edits.
        await Assertions.Expect(page.ActivityForm().Locator($".workflow-designer-form[data-activity-id='{activityId}']")).ToBeVisibleAsync();
    }

    /// <summary>
    /// Publishes the draft, confirming the dialog that warnings or running instances bring up.
    /// </summary>
    public static async Task PublishAsync(this IPage page)
    {
        await page.WaitForSavedAsync();
        await page.Locator("[data-cy=toolbar-publish]").ClickAsync();

        var confirm = page.Locator("[data-cy=publish-dialog-confirm]");
        var discard = page.Locator("[data-cy=toolbar-discard]");
        await Assertions.Expect(confirm.Or(page.Locator("[data-cy=toast]", new() { HasTextString = "published" }))).ToBeVisibleAsync();

        if (await confirm.IsVisibleAsync())
        {
            await confirm.ClickAsync();
        }

        // Without a draft there is nothing to discard.
        await Assertions.Expect(discard).ToBeDisabledAsync();
    }

    /// <summary>
    /// Generates the URL of an HTTP request event (as its editor does) and returns it, relative to the site.
    /// </summary>
    public static async Task<string> GenerateHttpUrlAsync(this IPage page, long workflowTypeId, string activityId)
        => await page.EvaluateAsync<string>(@"async ([workflowTypeId, activityId]) => {
            const token = document.querySelector('input[name=__RequestVerificationToken]').value;
            const response = await fetch(`/Admin/OrchardCore.Workflows/HttpWorkflow/GenerateUrl?workflowTypeId=${workflowTypeId}&activityId=${activityId}&tokenLifeSpan=1`, { method: 'POST', headers: { RequestVerificationToken: token } });
            // The action returns the URL as plain text.
            return (await response.text()).replace(/^""|""$/g, '');
        }", new object[] { workflowTypeId, activityId });

    private static long ParseWorkflowTypeId(string url)
    {
        var path = url.Split('?')[0].TrimEnd('/');

        return long.Parse(path[(path.LastIndexOf('/') + 1)..], CultureInfo.InvariantCulture);
    }
}
