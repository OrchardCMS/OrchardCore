using System.Text.RegularExpressions;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// The OrchardCore.Workflows designer: the toolbox, canvas and properties panel, drafts and publishing, and the
// read-only instance viewer. The recipe seeds the "Seeded approval" workflow (HTTP request → Notify → Signal →
// Notify), the workflows of the version and variable tests, and a WorkflowViewer role that can open the admin but
// can't manage workflows. Other tests that change a workflow create their own.
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
        await page.ActivityForm().Locator("select[name='IfElseTask.Condition.Syntax']").SelectOptionAsync("Liquid");
        var condition = page.ActivityForm().Locator("[name='IfElseTask.Condition.Expression']");
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
    public async Task Versions_InstanceWaitingWhileVersionTwoIsPublished_FinishesOnVersionOne()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync("Versioned approval");
        await page.OpenDesignerAsync(id);
        await Assertions.Expect(page.Locator("[data-cy=published-version]")).ToHaveTextAsync("Version 1");

        // An instance starts on version 1, returns the URL of the signal it waits for, and waits.
        var startUrl = await page.GenerateHttpUrlAsync(id, "versionstart");
        var started = await page.APIRequest.GetAsync(startUrl);
        Assert.True(started.Status < 400, $"Expected the workflow to start, got {started.Status}.");
        var signalUrl = (await started.TextAsync()).Trim();
        Assert.Contains("trigger", signalUrl, StringComparison.OrdinalIgnoreCase);

        // Version 2 no longer has the activity the instance waits on.
        await page.Activity("versionwait").Locator(".wfd-node-header").ClickAsync();
        await page.Keyboard.PressAsync("Delete");
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(3);
        await page.PublishAsync();
        await Assertions.Expect(page.Locator("[data-cy=published-version]")).ToHaveTextAsync("Version 2");

        // The instance still runs on version 1.
        await page.GotoAndAssertOkAsync($"/Admin/Workflows/Types/{id}/Instances/Index");
        await Assertions.Expect(page.Locator("[data-cy=instance-version]")).ToHaveTextAsync("Version 1");
        await page.Locator("a[href*='/Workflow/Details/']").First.ClickAsync();
        await page.WaitForDesignerAsync();
        await Assertions.Expect(page.Locator("[data-cy=instance-version]")).ToContainTextAsync("1");
        await Assertions.Expect(page.Locator("[data-cy=instance-version-not-published]")).ToBeVisibleAsync();
        await Assertions.Expect(page.Activities()).ToHaveCountAsync(4);
        await Assertions.Expect(page.Activity("versionwait")).ToHaveClassAsync(new Regex("is-blocking"));

        // The signal still resumes it, and it finishes on version 1.
        var resumed = await page.APIRequest.GetAsync(signalUrl);
        Assert.True(resumed.Status < 400, $"Expected the workflow to resume, got {resumed.Status}.");

        await page.GotoAndAssertOkAsync($"/Admin/Workflows/Types/{id}/Instances/Index");
        await Assertions.Expect(page.Locator(".list-group-item", new() { Has = page.Locator("[data-cy=instance-version]") }).Locator(".badge").First).ToContainTextAsync("Finished");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Versions_History_ComparesAndRestoresAnEarlierVersion()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync("Versioned history");
        await page.OpenDesignerAsync(id);

        // Version 2 moves the notification one grid cell to the right.
        await page.Activity("historynotify").Locator(".wfd-node-header").ClickAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        await Assertions.Expect(page.Activity("historynotify")).Not.ToHaveAttributeAsync("data-x", "360");
        await page.PublishAsync();
        await Assertions.Expect(page.Locator("[data-cy=published-version]")).ToHaveTextAsync("Version 2");

        await page.Locator("[data-cy=toolbar-versions]").ClickAsync();
        await Assertions.Expect(page.Locator("[data-cy=version-2] [data-cy=version-published]")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-cy=version-1] [data-cy=version-restore]")).ToBeVisibleAsync();

        // Version 1 compared with the published version: the notification moved.
        await page.Locator("[data-cy=version-1] [data-cy=version-compare]").ClickAsync();
        await page.WaitForURLAsync("**/CompareVersions/**");
        await Assertions.Expect(page.Locator("[data-cy=compare-to] [data-cy=activity-historynotify]")).ToHaveClassAsync(new Regex("is-moved"));
        await Assertions.Expect(page.Locator("[data-cy=compare-group-moved]")).ToBeVisibleAsync();

        // Restoring version 1 puts it in the draft; version 2 stays published until the draft is.
        await page.OpenDesignerAsync(id);
        await page.Locator("[data-cy=toolbar-versions]").ClickAsync();
        await page.Locator("[data-cy=version-1] [data-cy=version-restore]").ClickAsync();
        await Assertions.Expect(page.Locator("[data-cy=version-1]")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Activity("historynotify")).ToHaveAttributeAsync("data-x", "360");
        await Assertions.Expect(page.Locator("[data-cy=toolbar-discard]")).ToBeEnabledAsync();
        await Assertions.Expect(page.Locator("[data-cy=published-version]")).ToHaveTextAsync("Version 2");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Variables_ScriptResultBoundToAVariable_TheResponseReadsIt()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync("Variables greeting");
        await page.OpenDesignerAsync(id);

        // Declare the variable.
        await page.Locator("[data-cy=panel-tab-variables]").ClickAsync();
        await page.Locator("[data-cy=variable-add]").ClickAsync();
        var name = page.Locator("[data-cy=variable] [data-cy=variable-name]").Last;
        await Assertions.Expect(name).ToBeFocusedAsync();
        await Assertions.Expect(page.Locator("[data-cy=variable] [data-cy=variable-type]").Last).ToHaveValueAsync("string");
        await name.FillAsync("greeting");
        await name.PressAsync("Tab");
        await page.WaitForSavedAsync();
        await Assertions.Expect(page.Locator("#wfd-variables option[value='greeting']")).ToHaveCountAsync(1);

        // Store the script's result in it.
        await page.Locator("[data-cy=panel-tab-activity]").ClickAsync();
        await page.EditActivityAsync("greetscript");
        var result = page.Locator("[data-cy=activity-outputs] [data-cy=output-Result] [data-cy=output-binding]");
        await result.SelectOptionAsync("greeting");
        await Assertions.Expect(result).ToHaveValueAsync("greeting");
        await page.PublishAsync();

        // The response reads the variable that the binding wrote.
        var url = await page.GenerateHttpUrlAsync(id, "greetstart");
        var response = await page.APIRequest.GetAsync(url);
        Assert.Equal(200, response.Status);
        Assert.Equal("hello 42", (await response.TextAsync()).Trim());

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ExpressionSyntax_LegacyLiquidValueChangedToACustomSyntax_TheResponseUsesIt()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync("Expression syntaxes");
        await page.OpenDesignerAsync(id);

        // The legacy activity runs its Liquid value, and the editor shows it that way.
        var url = await page.GenerateHttpUrlAsync(id, "syntaxstart");
        Assert.Equal("legacy", (await (await page.APIRequest.GetAsync(url)).TextAsync()).Trim());

        await page.EditActivityAsync("syntaxset");
        var value = page.ActivityForm().Locator("[data-workflow-expression]").Last;
        var syntax = value.Locator("select[name='SetVariableTask.Value.Syntax']");
        await Assertions.Expect(syntax).ToHaveValueAsync("Liquid");
        await Assertions.Expect(syntax.Locator("option")).ToHaveTextAsync(["Literal", "Liquid", "JavaScript", "Upper case"]);
        await Assertions.Expect(value.Locator(".monaco-editor")).ToBeVisibleAsync();
        await Assertions.Expect(value.Locator("textarea[name='SetVariableTask.Value.Expression']")).ToHaveValueAsync("{{ 'legacy' }}");

        // The module's syntax, with a value typed in the code editor.
        await syntax.SelectOptionAsync("UpperCase");
        await value.Locator(".monaco-editor").ClickAsync();
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync("world");
        await page.Locator("[data-cy=panel-activity-title]").ClickAsync();
        await Assertions.Expect(value.Locator("textarea[name='SetVariableTask.Value.Expression']")).ToHaveValueAsync("world");
        await page.PublishAsync();

        Assert.Equal("WORLD", (await (await page.APIRequest.GetAsync(url)).TextAsync()).Trim());

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Journal_FaultedInstance_ShowsWhatRanAndIsRetried()
    {
        var (page, consoleErrors) = await OpenAsync();
        var id = await page.FindWorkflowTypeIdAsync("Transient failure");

        // The first run faults on the transient failure.
        var url = await page.GenerateHttpUrlAsync(id, "faultstart");
        await page.APIRequest.GetAsync(url);

        // The instance page shows what ran, and where it faulted.
        await page.GotoAndAssertOkAsync($"/Admin/Workflows/Types/{id}/Instances/Index");
        await page.Locator("a[href*='/Workflow/Details/']").First.ClickAsync();
        await page.WaitForDesignerAsync();
        await Assertions.Expect(page.Activity("faultstart")).ToHaveClassAsync(new Regex("is-executed"));
        await Assertions.Expect(page.Activity("faultcall")).ToHaveClassAsync(new Regex("is-faulted"));
        await Assertions.Expect(page.Activity("faultdone")).Not.ToHaveClassAsync(new Regex("is-executed"));
        await Assertions.Expect(page.Edge("faultstart", "Done", "faultcall")).ToHaveClassAsync(new Regex("is-executed"));

        await page.Locator("[data-cy=panel-tab-journal]").ClickAsync();
        await Assertions.Expect(page.Locator("[data-cy=journal-record-2] [data-cy=journal-error]")).ToContainTextAsync("briefly unavailable");

        // Retrying from the faulted activity runs it again, and the instance finishes.
        await page.Locator("[data-cy=journal-record-2] button").ClickAsync();
        await page.Locator("[data-cy=panel-tab-activity]").ClickAsync();
        await Assertions.Expect(page.Locator("[data-cy=fault-message]")).ToContainTextAsync("briefly unavailable");
        await page.Locator("[data-cy=retry-button]").ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();

        await Assertions.Expect(page.Locator("[data-cy=toast]")).ToContainTextAsync("Finished");
        await Assertions.Expect(page.Activity("faultdone")).ToHaveClassAsync(new Regex("is-executed"));
        await Assertions.Expect(page.Activity("faultcall")).Not.ToHaveClassAsync(new Regex("is-faulted"));
        await Assertions.Expect(page.Locator("[data-cy=retry]")).ToHaveCountAsync(0);

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
