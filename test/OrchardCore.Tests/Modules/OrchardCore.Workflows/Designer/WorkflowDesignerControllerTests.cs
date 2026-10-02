using System.Text.Json;
using System.Text.Json.Nodes;
using AngleSharp.Html.Parser;
using OrchardCore.Admin;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Designer;

public sealed class WorkflowDesignerControllerTests : IClassFixture<WorkflowDesignerSiteFixture>
{
    private readonly WorkflowDesignerSiteFixture _fixture;

    public WorkflowDesignerControllerTests(WorkflowDesignerSiteFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DesignerEndpoints_WithoutManageWorkflows_ReturnForbiddenProblem()
    {
        using var context = new SiteContext
        {
            PermissionsContext = new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = [AdminPermissions.AccessAdminPanel],
            },
        };
        await context.InitializeAsync();
        await WorkflowDesignerSiteFixture.EnableFeaturesAsync(context, "OrchardCore.Workflows");
        var (id, _) = await WorkflowDesignerSiteFixture.CreateWorkflowTypeAsync(context, Activity("notify", "NotifyTask"));

        foreach (var action in new[] { "Definition", "Library", "Editor?activityId=notify", "Settings" })
        {
            using var response = await context.Client.GetAsync($"Admin/Workflows/Types/{id}/Designer/{action}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var problem = await ReadJsonAsync(response);
            Assert.Equal(403, problem["status"].GetValue<int>());
        }
    }

    [Fact]
    public async Task Index_ManageWorkflows_RendersDesignerWithTenantAwareConfig()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/{id}/Designer/Index", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var host = Assert.Single(document.QuerySelectorAll("#workflow-designer"));
        var config = JsonNode.Parse(host.GetAttribute("data-config"));
        var tenantPrefix = $"/{_fixture.Context.TenantName}/";

        Assert.Equal(id, config["workflowTypeId"].GetValue<long>());
        Assert.False(config["readOnly"].GetValue<bool>());
        Assert.Equal($"{tenantPrefix}Admin/Workflows/Types/{id}/Designer/Definition", config["urls"]["definition"].GetValue<string>());
        Assert.Equal($"{tenantPrefix}Admin/Workflows/Types/{id}/Designer/Editor", config["urls"]["editor"].GetValue<string>());
        Assert.False(string.IsNullOrEmpty(config["translations"]["Undo"].GetValue<string>()));
        Assert.NotNull(document.QuerySelector("input[name='__RequestVerificationToken']"));
        Assert.Contains(document.QuerySelectorAll("script[src]"), x => x.GetAttribute("src").Contains("workflows-designer") && x.GetAttribute("type") == "module");
    }

    [Fact]
    public async Task Library_RegisteredActivities_ReturnsRegistrationIconsAndCategoryDefaults()
    {
        var (id, _) = await CreateWorkflowTypeAsync();

        var library = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Library");
        var activities = library["categories"].AsArray().SelectMany(x => x["activities"].AsArray()).ToDictionary(x => x["name"].GetValue<string>());

        // Set on the registration of a built-in Workflows activity.
        Assert.Equal("fa-solid fa-bell", activities["NotifyTask"]["icon"].GetValue<string>());
        Assert.Equal("fa-solid fa-globe", activities["HttpRequestEvent"]["icon"].GetValue<string>());
        // Not set by its module (Forms), so the default of its category applies.
        Assert.Equal("fa-solid fa-check-double", activities["ValidateFormTask"]["icon"].GetValue<string>());
        Assert.False(string.IsNullOrWhiteSpace(activities["NotifyTask"]["thumbnailHtml"].GetValue<string>()));
        Assert.True(activities["HttpRequestEvent"]["isEvent"].GetValue<bool>());
    }

    [Fact]
    public async Task Definition_NoDraftThenSave_ReturnsLiveThenDraftGraph()
    {
        var (id, _) = await CreateWorkflowTypeAsync(
            Activity("start", "HttpRequestEvent", isStart: true, x: 10, y: 20),
            Activity("notify", "NotifyTask", x: 300, y: 20));

        var live = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");

        Assert.False(live["hasDraft"].GetValue<bool>());
        Assert.Equal(0, live["revision"].GetValue<int>());
        Assert.Equal(["start", "notify"], live["nodes"].AsArray().Select(x => x["id"].GetValue<string>()));
        var start = live["nodes"][0];
        Assert.Equal(10, start["x"].GetValue<int>());
        Assert.True(start["isStart"].GetValue<bool>());
        Assert.True(start["isEvent"].GetValue<bool>());
        Assert.Contains("Done", start["outcomes"].AsArray().Select(x => x["name"].GetValue<string>()));
        Assert.False(string.IsNullOrWhiteSpace(start["designHtml"].GetValue<string>()));
        Assert.Equal("fa-solid fa-globe", start["icon"].GetValue<string>());
        Assert.Contains(live["issues"].AsArray(), x => x["code"].GetValue<string>() == "UnreachableActivity" && x["severity"].GetValue<string>() == "Warning");

        using var saved = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Save", new
        {
            revision = 0,
            nodes = new[]
            {
                new { id = "start", x = 40, y = 50, isStart = true },
                new { id = "notify", x = 300, y = 20, isStart = false },
            },
            transitions = new[] { new { sourceActivityId = "start", sourceOutcomeName = "Done", destinationActivityId = "notify" } },
            removedActivityIds = Array.Empty<string>(),
        });

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var savedJson = await ReadJsonAsync(saved);
        Assert.Equal(1, savedJson["revision"].GetValue<int>());
        Assert.Empty(savedJson["issues"].AsArray());

        var draft = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");

        Assert.True(draft["hasDraft"].GetValue<bool>());
        Assert.Equal(1, draft["revision"].GetValue<int>());
        Assert.Equal(40, draft["nodes"][0]["x"].GetValue<int>());
        Assert.Equal("notify", Assert.Single(draft["transitions"].AsArray())["destinationActivityId"].GetValue<string>());
    }

    [Fact]
    public async Task Save_StaleRevision_ReturnsConflictProblem()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));
        var body = new { revision = 0, nodes = new[] { new { id = "start", x = 5, y = 5, isStart = true } }, transitions = Array.Empty<object>(), removedActivityIds = Array.Empty<string>() };

        using (var first = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Save", body))
        {
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        }

        using var stale = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Save", body);

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("application/problem+json", stale.Content.Headers.ContentType?.MediaType);
        var problem = await ReadJsonAsync(stale);
        Assert.Equal(1, problem["currentRevision"].GetValue<int>());
        Assert.NotNull(problem["modifiedUtc"]);
    }

    [Fact]
    public async Task AddActivity_ActivityWithoutEditor_AddsActivity()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync();

        using var response = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/AddActivity", new { revision = 0, name = "ValidateAntiforgeryTokenTask", x = 120, y = 80 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal(1, json["revision"].GetValue<int>());
        var node = json["node"];
        Assert.Equal("ValidateAntiforgeryTokenTask", node["name"].GetValue<string>());
        Assert.False(node["hasEditor"].GetValue<bool>());
        Assert.False(node["isStart"].GetValue<bool>());
        Assert.Equal(120, node["x"].GetValue<int>());

        await _fixture.Context.UsingTenantScopeAsync(async scope =>
        {
            var draft = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeDraftManager>().GetAsync(workflowTypeId);
            Assert.Equal(node["id"].GetValue<string>(), Assert.Single(draft.Activities).ActivityId);

            var live = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().GetAsync(workflowTypeId);
            Assert.Empty(live.Activities);
        });
    }

    [Fact]
    public async Task Editor_ScriptTask_ReturnsContentAndScripts()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("script", "ScriptTask"));

        var json = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Editor?activityId=script");

        Assert.True(json["valid"].GetValue<bool>());
        var content = json["content"].GetValue<string>();
        Assert.Contains($"data-workflow-type-id=\"{id}\"", content);
        Assert.Contains("data-activity-id=\"script\"", content);
        Assert.Contains("ScriptTask.Script", content);
        var scripts = json["scripts"].GetValue<string>();
        Assert.Contains("monaco-loader", scripts);
        Assert.Contains("workflow-monaco-text-editor", scripts);
        Assert.Contains("monaco-loader", json["styles"].GetValue<string>());
    }

    [Fact]
    public async Task EditorPost_InvalidLiquidIfElse_ReturnsInvalidContentWithError()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("if", "IfElseTask"));

        using var response = await PostFormAsync($"Admin/Workflows/Types/{id}/Designer/Editor?activityId=if&revision=0", new Dictionary<string, string>
        {
            ["IActivity.ActivityMetadata.Title"] = string.Empty,
            ["IfElseTask.Syntax"] = "Liquid",
            ["IfElseTask.ConditionExpression"] = string.Empty,
            ["IfElseTask.LiquidConditionExpression"] = "{{ true ",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.False(json["valid"].GetValue<bool>());

        var document = new HtmlParser().ParseDocument(json["content"].GetValue<string>());
        var error = Assert.Single(document.QuerySelectorAll(".field-validation-error"));
        Assert.Contains("Liquid", error.TextContent);
        Assert.Equal("Liquid", document.QuerySelector("select[name='IfElseTask.Syntax'] option[selected]")?.GetAttribute("value"));

        await _fixture.Context.UsingTenantScopeAsync(async scope =>
        {
            Assert.Null(await scope.ServiceProvider.GetRequiredService<IWorkflowTypeDraftManager>().GetAsync(workflowTypeId));
        });
    }

    [Fact]
    public async Task EditorPost_ForkBranchesChanged_UpdatesOutcomesAndRemovesTransitions()
    {
        var (id, _) = await CreateWorkflowTypeAsync(
            [
                Activity("start", "HttpRequestEvent", isStart: true),
                Activity("fork", "ForkTask", properties: new JsonObject { ["Forks"] = new JsonArray("A", "B") }),
                Activity("a", "NotifyTask"),
                Activity("b", "NotifyTask"),
            ],
            [
                Transition("start", "Done", "fork"),
                Transition("fork", "A", "a"),
                Transition("fork", "B", "b"),
            ]);

        using var response = await PostFormAsync($"Admin/Workflows/Types/{id}/Designer/Editor?activityId=fork&revision=0", new Dictionary<string, string>
        {
            ["IActivity.ActivityMetadata.Title"] = "Split",
            ["ForkTask.Forks"] = "A, C",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.True(json["valid"].GetValue<bool>());
        Assert.Equal(1, json["revision"].GetValue<int>());
        Assert.Equal(["A", "C"], json["node"]["outcomes"].AsArray().Select(x => x["name"].GetValue<string>()));
        Assert.Equal("Split", json["node"]["title"].GetValue<string>());
        var removed = Assert.Single(json["removedTransitions"].AsArray());
        Assert.Equal("B", removed["sourceOutcomeName"].GetValue<string>());

        var definition = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        Assert.DoesNotContain(definition["transitions"].AsArray(), x => x["sourceOutcomeName"].GetValue<string>() == "B");
    }

    [Fact]
    public async Task Publish_DraftWithHttpEvents_RegistersRoutesAndDeletesDraft()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync();

        using (var addFilter = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/AddActivity", new { revision = 0, name = "HttpRequestFilterEvent", x = 0, y = 0 }))
        {
            Assert.Equal(HttpStatusCode.OK, addFilter.StatusCode);
        }

        string requestEventId = null;

        using (var addRequest = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/AddActivity", new { revision = 1, name = "HttpRequestEvent", x = 0, y = 200 }))
        {
            requestEventId = (await ReadJsonAsync(addRequest))["node"]["id"].GetValue<string>();
        }

        // Both are start activities: the filter event because it was the first event, the request event set here.
        var definition = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        using (var save = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Save", new
        {
            revision = 2,
            nodes = definition["nodes"].AsArray().Select(x => new { id = x["id"].GetValue<string>(), x = 0, y = 0, isStart = true }).ToArray(),
            transitions = Array.Empty<object>(),
            removedActivityIds = Array.Empty<string>(),
        }))
        {
            Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        }

        using (var edit = await PostFormAsync($"Admin/Workflows/Types/{id}/Designer/Editor?activityId={requestEventId}&revision=3", new Dictionary<string, string>
        {
            ["HttpRequestEvent.HttpMethod"] = "GET",
            ["HttpRequestEvent.ValidateAntiforgeryToken"] = "false",
            ["HttpRequestEvent.TokenLifeSpan"] = "0",
        }))
        {
            Assert.True((await ReadJsonAsync(edit))["valid"].GetValue<bool>());
        }

        var invokeUrl = await _fixture.CreateInvokeUrlAsync(workflowTypeId, requestEventId);

        using (var beforePublish = await _fixture.Context.Client.GetAsync(invokeUrl, TestContext.Current.CancellationToken))
        {
            Assert.Equal(HttpStatusCode.NotFound, beforePublish.StatusCode);
        }

        Assert.Empty(await _fixture.GetRouteEntriesAsync(id));

        using var publish = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Publish", new { revision = 4 });

        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        Assert.NotNull((await ReadJsonAsync(publish))["publishedUtc"]);
        Assert.Single(await _fixture.GetRouteEntriesAsync(id));

        using (var afterPublish = await _fixture.Context.Client.GetAsync(invokeUrl, TestContext.Current.CancellationToken))
        {
            Assert.NotEqual(HttpStatusCode.NotFound, afterPublish.StatusCode);
        }

        var published = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        Assert.False(published["hasDraft"].GetValue<bool>());
        Assert.Equal(2, published["nodes"].AsArray().Count);
    }

    [Fact]
    public async Task Discard_Draft_RevertsToLiveDefinition()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));

        using (var add = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/AddActivity", new { revision = 0, name = "NotifyTask", x = 0, y = 0 }))
        {
            Assert.Equal(HttpStatusCode.OK, add.StatusCode);
        }

        Assert.Equal(2, (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["nodes"].AsArray().Count);

        using var discard = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Discard", new { });

        Assert.Equal(HttpStatusCode.OK, discard.StatusCode);
        var definition = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        Assert.False(definition["hasDraft"].GetValue<bool>());
        Assert.Equal("start", Assert.Single(definition["nodes"].AsArray())["id"].GetValue<string>());
    }

    [Fact]
    public async Task Save_WithoutAntiforgeryToken_IsRejected()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));

        using var response = await _fixture.Context.Client.PostAsync(
            $"Admin/Workflows/Types/{id}/Designer/Save",
            new StringContent("""{"revision":0,"nodes":[],"transitions":[],"removedActivityIds":[]}""", Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await _fixture.Context.UsingTenantScopeAsync(async scope =>
        {
            Assert.Null(await scope.ServiceProvider.GetRequiredService<IWorkflowTypeDraftManager>().GetAsync(workflowTypeId));
        });
    }

    private Task<(long Id, string WorkflowTypeId)> CreateWorkflowTypeAsync(params ActivityRecord[] activities)
        => WorkflowDesignerSiteFixture.CreateWorkflowTypeAsync(_fixture.Context, activities);

    private Task<(long Id, string WorkflowTypeId)> CreateWorkflowTypeAsync(ActivityRecord[] activities, Transition[] transitions)
        => WorkflowDesignerSiteFixture.CreateWorkflowTypeAsync(_fixture.Context, activities, transitions);

    private async Task<JsonNode> GetJsonAsync(string path)
    {
        using var response = await _fixture.Context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await ReadJsonAsync(response);
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string path, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
        await _fixture.AddAntiforgeryAsync(request);

        return await _fixture.Context.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> PostFormAsync(string path, Dictionary<string, string> fields)
    {
        var content = new MultipartFormDataContent();

        foreach (var (name, value) in fields)
        {
            content.Add(new StringContent(value), name);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        await _fixture.AddAntiforgeryAsync(request);

        return await _fixture.Context.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<JsonNode> ReadJsonAsync(HttpResponseMessage response)
        => JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

    private static ActivityRecord Activity(string id, string name, bool isStart = false, int x = 0, int y = 0, JsonObject properties = null)
        => new() { ActivityId = id, Name = name, IsStart = isStart, X = x, Y = y, Properties = properties ?? [] };

    private static Transition Transition(string source, string outcome, string destination)
        => new() { SourceActivityId = source, SourceOutcomeName = outcome, DestinationActivityId = destination };
}

public sealed class WorkflowDesignerSiteFixture : IAsyncLifetime
{
    private string _antiforgeryToken;
    private string _antiforgeryCookie;

    public SiteContext Context { get; } = new();

    public async ValueTask InitializeAsync()
    {
        await Context.InitializeAsync();
        await EnableFeaturesAsync(Context, "OrchardCore.Workflows", "OrchardCore.Workflows.Http", "OrchardCore.Forms");
    }

    public ValueTask DisposeAsync()
    {
        Context.Dispose();

        return ValueTask.CompletedTask;
    }

    // The test client doesn't keep cookies, so the antiforgery cookie issued with the token is sent explicitly.
    public async Task AddAntiforgeryAsync(HttpRequestMessage request)
    {
        if (_antiforgeryToken is null)
        {
            using var response = await Context.Client.GetAsync("Admin/Workflows/Types/EditProperties", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            _antiforgeryToken = document.QuerySelector("input[name='__RequestVerificationToken']").GetAttribute("value");
            _antiforgeryCookie = string.Join("; ", response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0]));
        }

        request.Headers.Add("RequestVerificationToken", _antiforgeryToken);
        request.Headers.Add("Cookie", _antiforgeryCookie);
    }

    public async Task<string> CreateInvokeUrlAsync(string workflowTypeId, string activityId)
    {
        string token = null;

        await Context.UsingTenantScopeAsync(scope =>
        {
            token = scope.ServiceProvider.GetRequiredService<ISecurityTokenService>()
                .CreateToken(new WorkflowPayload(workflowTypeId, activityId), TimeSpan.FromDays(1));

            return Task.CompletedTask;
        });

        return "workflows/invoke/" + Uri.EscapeDataString(token);
    }

    public async Task<IList<WorkflowRoutesEntry>> GetRouteEntriesAsync(long workflowTypeId)
    {
        IList<WorkflowRoutesEntry> entries = [];

        await Context.UsingTenantScopeAsync(async scope =>
        {
            var document = await scope.ServiceProvider.GetRequiredService<IVolatileDocumentManager<WorkflowTypeRouteDocument>>().GetOrCreateImmutableAsync();

            if (document.Entries.TryGetValue(workflowTypeId.ToString(CultureInfo.InvariantCulture), out var found))
            {
                entries = found;
            }
        });

        return entries;
    }

    public static async Task<(long Id, string WorkflowTypeId)> CreateWorkflowTypeAsync(SiteContext context, params ActivityRecord[] activities)
        => await CreateWorkflowTypeAsync(context, activities, []);

    public static async Task<(long Id, string WorkflowTypeId)> CreateWorkflowTypeAsync(SiteContext context, ActivityRecord[] activities, Transition[] transitions)
    {
        var workflowType = new WorkflowType
        {
            WorkflowTypeId = Guid.NewGuid().ToString("n"),
            Name = "Designer test",
            IsEnabled = true,
            Activities = activities,
            Transitions = transitions,
        };

        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().SaveAsync(workflowType));

        return (workflowType.Id, workflowType.WorkflowTypeId);
    }

    public static async Task EnableFeaturesAsync(SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await featuresManager.GetAvailableFeaturesAsync();
            var features = availableFeatures.Where(feature => featureIds.Contains(feature.Id)).ToArray();
            Assert.Equal(featureIds.Length, features.Length);
            await featuresManager.EnableFeaturesAsync(features, force: true);
        });

        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
    }
}
