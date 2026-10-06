using System.Text.RegularExpressions;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// The OrchardCore.Workflows designer: the toolbox, canvas and properties panel, drafts and publishing, and the
// read-only instance viewer. The recipe seeds the "Seeded approval" workflow (HTTP request → Notify → Signal →
// Notify) and a WorkflowViewer role that can open the admin but can't manage workflows. Tests that change a
// workflow create their own.
public sealed class WorkflowsDesignerTests : CmsTestBase<WorkflowsDesignerTestsFixture>, IClassFixture<WorkflowsDesignerTestsFixture>
{
    private const string SeededWorkflow = "Seeded approval";

    private static readonly Regex s_selected = new("is-selected");

    public WorkflowsDesignerTests(WorkflowsDesignerTestsFixture fixture) : base(fixture) { }

    private async Task<(IPage Page, List<string> ConsoleErrors)> OpenAsync()
    {
        var page = await Fixture.CreatePageAsync();

        // Room for the toolbox, the canvas and the properties panel next to the admin menu.
        await page.SetViewportSizeAsync(1600, 1000);
        await page.LoginAsync();

        return (page, page.CollectConsoleErrors());
    }

    [Fact]
    public async Task Open_SeededWorkflow_RendersTheDesignerWithTheStoredLayout()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync(SeededWorkflow);

        await page.OpenDesignerAsync(id);

        await Assertions.Expect(page.Locator("[data-cy=designer-toolbox]")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-cy=designer-panel]")).ToBeVisibleAsync();
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(4);
        await Assertions.Expect(page.Edges()).ToHaveCountAsync(3);

        foreach (var (activityId, x, y) in new[] { ("seededrequest", "40", "40"), ("seedednotify", "360", "40"), ("seededsignal", "360", "240"), ("seededapproved", "680", "240") })
        {
            await Assertions.Expect(page.Activity(activityId)).ToHaveAttributeAsync("data-x", x);
            await Assertions.Expect(page.Activity(activityId)).ToHaveAttributeAsync("data-y", y);
        }

        await Assertions.Expect(page.Activity("seededrequest").Locator("[data-cy=start-badge]")).ToBeVisibleAsync();
        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ToolboxDrag_ContentPublishedEvent_AddsTheStartActivityAndOpensItsEditor()
    {
        var (page, consoleErrors) = await OpenAsync();
        await page.CreateWorkflowTypeAsync("Toolbox drag");

        var eventId = await page.AddActivityAsync("ContentPublishedEvent", "Content Published", 200, 120);

        // The first event of a workflow is its start activity.
        await Assertions.Expect(page.Activity(eventId).Locator("[data-cy=start-badge]")).ToBeVisibleAsync();
        await Assertions.Expect(page.Activity(eventId)).ToHaveClassAsync(s_selected);
        await Assertions.Expect(page.Locator("[data-cy=panel-activity-title]")).ToContainTextAsync("Content Published");
        await Assertions.Expect(page.ActivityForm().Locator("form")).ToBeVisibleAsync();
        await page.WaitForSavedAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task PanelEdit_NotifyMessage_UpdatesTheActivityWithoutLeavingTheDesigner()
    {
        var (page, consoleErrors) = await OpenAsync();
        await page.CreateWorkflowTypeAsync("Panel edit");
        var designerUrl = page.Url;

        var eventId = await page.AddActivityAsync("ContentPublishedEvent", "Content Published", 200, 80);
        var notifyId = await page.AddActivityAsync("NotifyTask", "Notify", 200, 320);
        await page.ConnectAsync(eventId, "Done", notifyId);

        await page.EditActivityAsync(notifyId);
        var message = page.ActivityForm().Locator("input[name='NotifyTask.Message']");
        await message.FillAsync("Hello from the designer test");
        await message.PressAsync("Tab");

        await Assertions.Expect(page.Activity(notifyId).Locator(".wfd-node-body")).ToContainTextAsync("Hello from the designer test");
        await page.WaitForSavedAsync();
        Assert.Equal(designerUrl, page.Url);
        await Assertions.Expect(page.Edges()).ToHaveCountAsync(1);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Autosave_ThenPublish_TheDraftSurvivesAReloadAndPublishingMakesItLive()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.CreateWorkflowTypeAsync("Autosave and publish");

        var eventId = await page.AddActivityAsync("ContentPublishedEvent", "Content Published", 200, 120);
        await page.WaitForSavedAsync();

        // The draft was saved without publishing.
        await page.OpenDesignerAsync(id);
        await Assertions.Expect(page.Activity(eventId)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-cy=toolbar-discard]")).ToBeEnabledAsync();
        var x = await page.Activity(eventId).GetAttributeAsync("data-x");

        await page.PublishAsync();

        await page.OpenDesignerAsync(id);
        await Assertions.Expect(page.Activity(eventId)).ToHaveAttributeAsync("data-x", x);
        await Assertions.Expect(page.Locator("[data-cy=toolbar-discard]")).ToBeDisabledAsync();
        await Assertions.Expect(page.Locator("[data-cy=toolbar-publish]")).ToBeDisabledAsync();
        await Assertions.Expect(page.Locator("[data-cy=draft-banner]")).ToHaveCountAsync(0);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Discard_Draft_RevertsToThePublishedDefinition()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.CreateWorkflowTypeAsync("Discard");
        var eventId = await page.AddActivityAsync("ContentPublishedEvent", "Content Published", 200, 80);
        await page.PublishAsync();

        await page.AddActivityAsync("NotifyTask", "Notify", 200, 320);
        await page.WaitForSavedAsync();
        await page.Locator("[data-cy=toolbar-discard]").ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();

        await Assertions.Expect(page.Activities()).ToHaveCountAsync(1);
        await Assertions.Expect(page.Activity(eventId)).ToBeVisibleAsync();

        await page.OpenDesignerAsync(id);
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(1);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Delete_ThenUndo_RestoresTheActivityAndItsConnection()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.CreateWorkflowTypeAsync("Delete and undo");
        var eventId = await page.AddActivityAsync("ContentPublishedEvent", "Content Published", 200, 80);
        var notifyId = await page.AddActivityAsync("NotifyTask", "Notify", 200, 320);
        await page.ConnectAsync(eventId, "Done", notifyId);

        await page.Activity(notifyId).Locator(".wfd-node-header").ClickAsync();
        await page.Keyboard.PressAsync("Delete");

        await Assertions.Expect(page.Activity(notifyId)).ToHaveCountAsync(0);
        await Assertions.Expect(page.Edges()).ToHaveCountAsync(0);

        // The removal is saved before it is undone; the draft keeps the activity to restore it.
        await page.WaitForSavedAsync();
        await page.Locator("[data-cy=toast-action]").ClickAsync();

        await Assertions.Expect(page.Activity(notifyId)).ToBeVisibleAsync();
        await Assertions.Expect(page.Edge(eventId, "Done", notifyId)).ToHaveCountAsync(1);
        await page.WaitForSavedAsync();

        await page.OpenDesignerAsync(id);
        await Assertions.Expect(page.Activity(notifyId)).ToBeVisibleAsync();
        await Assertions.Expect(page.Edge(eventId, "Done", notifyId)).ToHaveCountAsync(1);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Connect_OutcomeAlreadyConnected_ReplacesItsTransition()
    {
        var (page, consoleErrors) = await OpenAsync();
        await page.CreateWorkflowTypeAsync("Replace connection");
        var eventId = await page.AddActivityAsync("ContentPublishedEvent", "Content Published", 200, 80);
        var firstId = await page.AddActivityAsync("NotifyTask", "Notify", 120, 320);
        var secondId = await page.AddActivityAsync("NotifyTask", "Notify", 440, 320);

        await page.ConnectAsync(eventId, "Done", firstId);
        await page.ConnectAsync(eventId, "Done", secondId);

        await Assertions.Expect(page.Edges()).ToHaveCountAsync(1);
        await Assertions.Expect(page.Edge(eventId, "Done", firstId)).ToHaveCountAsync(0);
        await page.WaitForSavedAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ForkBranches_Changed_UpdatesThePortsAndRemovesTheirConnections()
    {
        var (page, consoleErrors) = await OpenAsync();
        await page.CreateWorkflowTypeAsync("Fork branches");
        var forkId = await page.AddActivityAsync("ForkTask", "Fork", 200, 80);
        var notifyId = await page.AddActivityAsync("NotifyTask", "Notify", 520, 320);

        await page.EditActivityAsync(forkId);
        var forks = page.ActivityForm().Locator("[name='ForkTask.Forks']");
        await forks.FillAsync("Left, Right");
        await forks.PressAsync("Tab");
        await Assertions.Expect(page.Port(forkId, "Left")).ToBeVisibleAsync();
        await Assertions.Expect(page.Port(forkId, "Right")).ToBeVisibleAsync();

        await page.ConnectAsync(forkId, "Left", notifyId);

        await forks.FillAsync("Right, Middle");
        await forks.PressAsync("Tab");

        await Assertions.Expect(page.Port(forkId, "Middle")).ToBeVisibleAsync();
        await Assertions.Expect(page.Port(forkId, "Left")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Edges()).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-cy=toast]").Last).ToContainTextAsync("removed");
        await page.WaitForSavedAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task HttpRequestEvent_UrlGeneratedInThePanel_RespondsOncePublished()
    {
        var (page, consoleErrors) = await OpenAsync();
        await page.CreateWorkflowTypeAsync("HTTP request URL");
        var requestId = await page.AddActivityAsync("HttpRequestEvent", "Http Request", 200, 120);

        await page.EditActivityAsync(requestId);
        await page.ActivityForm().Locator("select[name='HttpRequestEvent.HttpMethod']").SelectOptionAsync("GET");
        await page.ActivityForm().Locator("#generate-url-button").ClickAsync();

        var urlField = page.ActivityForm().Locator("#workflow-url-text");
        await Assertions.Expect(urlField).ToHaveValueAsync(new Regex("/workflows/Invoke\\?token="));
        var url = await urlField.InputValueAsync();
        await Assertions.Expect(page.Activity(requestId).Locator(".wfd-node-body")).ToContainTextAsync("GET");
        await page.PublishAsync();

        // Publishing went through the store, so its handlers registered the route.
        var response = await page.APIRequest.GetAsync(url);
        Assert.NotEqual(404, response.Status);
        Assert.True(response.Status < 400, $"Expected the workflow to accept the request, got {response.Status}.");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task IfElse_InvalidLiquid_ShowsTheErrorAndKeepsTheActivitySelected()
    {
        var (page, consoleErrors) = await OpenAsync();
        await page.CreateWorkflowTypeAsync("Invalid If/Else");
        var otherId = await page.AddActivityAsync("NotifyTask", "Notify", 520, 320);
        var ifElseId = await page.AddActivityAsync("IfElseTask", "If Else", 200, 80);

        await page.EditActivityAsync(ifElseId);
        await page.ActivityForm().Locator("select[name='IfElseTask.Syntax']").SelectOptionAsync("Liquid");
        var condition = page.ActivityForm().Locator("[name='IfElseTask.LiquidConditionExpression']");
        await condition.FillAsync("{{ true ");
        await condition.PressAsync("Tab");

        await Assertions.Expect(page.ActivityForm().Locator(".field-validation-error")).ToContainTextAsync("Liquid");

        // Leaving the activity asks first; keeping the changes keeps it selected.
        await page.Activity(otherId).Locator(".wfd-node-header").ClickAsync();
        await page.Locator("#modalCancelButton").ClickAsync();

        await Assertions.Expect(page.Activity(ifElseId)).ToHaveClassAsync(s_selected);
        await Assertions.Expect(page.ActivityForm().Locator(".field-validation-error")).ToBeVisibleAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task TwoPages_SameDraft_TheOutdatedOneGetsTheConflictDialog()
    {
        var (first, firstErrors) = await OpenAsync();
        var id = await first.CreateWorkflowTypeAsync("Concurrent editing");
        var (second, secondErrors) = await OpenAsync();
        await second.OpenDesignerAsync(id);

        await first.AddActivityAsync("ContentPublishedEvent", "Content Published", 200, 80);
        await first.WaitForSavedAsync();

        // The second page still has the revision it loaded.
        await second.Locator("[data-cy=toolbox-search]").FillAsync("Notify");
        await second.Locator("[data-cy='toolbox-activity-NotifyTask']").DragToAsync(second.CanvasSurface(), new() { TargetPosition = new() { X = 200, Y = 320 } });

        // The dialog's host has no box of its own (the modal is fixed), so its buttons are checked.
        await Assertions.Expect(second.Locator("[data-cy=conflict-dialog] [data-cy=conflict-reload]")).ToBeVisibleAsync();
        await Assertions.Expect(second.Locator("[data-cy=conflict-dialog] [data-cy=conflict-overwrite]")).ToBeVisibleAsync();
        await Assertions.Expect(second.Locator("[data-cy=save-status]")).ToHaveAttributeAsync("data-status", "conflict");

        Assert.Empty(firstErrors);
        // The browser reports the rejected request itself.
        Assert.All(secondErrors, error => Assert.Contains("409", error));
        await first.CloseAsync();
        await second.CloseAsync();
    }

    [Fact]
    public async Task InstancePage_HaltedInstance_ShowsTheReadOnlyDesignerWithTheBlockingActivity()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync(SeededWorkflow);
        await page.OpenDesignerAsync(id);

        // Start an instance: it halts on the signal.
        var url = await page.GenerateHttpUrlAsync(id, "seededrequest");
        var response = await page.APIRequest.GetAsync(url);
        Assert.True(response.Status < 400, $"Expected the workflow to start, got {response.Status}.");

        await page.GotoAndAssertOkAsync($"/Admin/Workflows/Types/{id}/Instances/Index");
        await page.Locator("a[href*='/Workflow/Details/']").First.ClickAsync();
        await page.WaitForDesignerAsync();

        await Assertions.Expect(page.Locator("[data-cy=designer-toolbox]")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-cy=toolbar-publish]")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-cy=viewer-legend]")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".wfd-node.is-blocking")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Activity("seededsignal")).ToHaveClassAsync(new Regex("is-blocking"));

        await page.Activity("seededsignal").Locator(".wfd-node-header").ClickAsync();
        await Assertions.Expect(page.Locator("[data-cy=panel-summary-blocking]")).ToContainTextAsync("Waiting");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task OldActivityEditUrl_RedirectsToTheDesignerWithTheActivityOpen()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync(SeededWorkflow);

        await page.GotoAndAssertOkAsync($"/Admin/Workflows/Types/{id}/Activity/seedednotify/Edit");
        await page.WaitForDesignerAsync();

        Assert.Contains($"/Admin/Workflows/Types/Edit/{id}?activityId=seedednotify", page.Url);
        await Assertions.Expect(page.Activity("seedednotify")).ToHaveClassAsync(s_selected);
        await Assertions.Expect(page.ActivityForm().Locator("input[name='NotifyTask.Message']")).ToHaveValueAsync("An article is waiting for review.");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DarkMode_Designer_RendersWithoutConsoleErrors()
    {
        var (page, consoleErrors) = await OpenAsync();
        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        var id = await page.FindWorkflowTypeIdAsync(SeededWorkflow);

        await page.OpenDesignerAsync(id);

        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark");
        var background = await page.Designer().EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        Assert.NotEqual("rgb(255, 255, 255)", background);

        await page.EditActivityAsync("seedednotify");
        await page.Locator("[data-cy=panel-tab-issues]").ClickAsync();
        await page.Locator("[data-cy=panel-tab-workflow]").ClickAsync();
        await Assertions.Expect(page.Locator("[data-cy=panel-settings-form] input[name='Name']")).ToHaveValueAsync(SeededWorkflow);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task CollapseActivity_SeededWorkflow_HidesWhatComesAfterItUntilShown()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync(SeededWorkflow);
        await page.OpenDesignerAsync(id);

        await page.Activity("seedednotify").Locator(".wfd-node-header").ClickAsync(new() { Button = MouseButton.Right });
        await page.Locator("[data-cy=menu-collapse]").ClickAsync();

        await Assertions.Expect(page.Activities()).ToHaveCountAsync(2);
        await Assertions.Expect(page.Edges()).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-cy=expand-seedednotify]")).ToHaveTextAsync("2");

        // The collapsed branch is remembered by the browser; the workflow itself doesn't change.
        await page.ReloadAsync();
        await page.WaitForDesignerAsync();
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(2);
        await Assertions.Expect(page.Locator("[data-cy=toolbar-discard]")).ToBeDisabledAsync();

        // The count under the collapsed activity expands its branch.
        await page.Locator("[data-cy=expand-seedednotify]").ClickAsync();
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(4);
        await Assertions.Expect(page.Locator("[data-cy=hidden-notice]")).ToHaveCountAsync(0);

        // Collapsing the start activity hides everything after it, until "Show all".
        await page.Activity("seededrequest").Locator(".wfd-node-header").ClickAsync(new() { Button = MouseButton.Right });
        await page.Locator("[data-cy=menu-collapse]").ClickAsync();
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-cy=expand-seededrequest]")).ToHaveTextAsync("3");

        await page.Locator("[data-cy=expand-all]").ClickAsync();
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(4);
        await Assertions.Expect(page.Locator("[data-cy=hidden-notice]")).ToHaveCountAsync(0);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DesignerEndpoints_UserWithoutManageWorkflows_ReturnForbidden()
    {
        var (page, _) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync(SeededWorkflow);
        await UserHelper.CreateUserAsync(page, string.Empty, "workflowviewer", "workflowviewer@orchard.com", TestUtils.DefaultConfig.Password, "WorkflowViewer");
        await UserHelper.LoginAsAsync(page, string.Empty, "workflowviewer", TestUtils.DefaultConfig.Password);

        foreach (var action in new[] { "Definition", "Library", "Editor?activityId=seedednotify", "Settings", $"Instance?instanceId=1" })
        {
            var response = await page.APIRequest.GetAsync($"/Admin/Workflows/Types/{id}/Designer/{action}");
            Assert.Equal(403, response.Status);
        }

        await page.CloseAsync();
    }
}
