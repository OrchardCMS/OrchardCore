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

        foreach (var action in new[] { "Definition", "Library", "Editor?activityId=notify", "Settings", "Versions", "Version?versionId=x", "Compare?from=draft&to=draft" })
        {
            using var response = await context.Client.GetAsync($"Admin/Workflows/Types/{id}/Designer/{action}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var problem = await ReadJsonAsync(response);
            Assert.Equal(403, problem["status"].GetValue<int>());
        }
    }

    [Fact]
    public async Task DesignerPosts_WithoutManageWorkflows_ReturnForbidden()
    {
        // The posts go through the fixture's client, which has an antiforgery token, with a permissions context
        // that only allows the admin panel.
        var permissionsContextKey = Guid.NewGuid().ToString("n");
        SiteStartup.PermissionsContexts.TryAdd(permissionsContextKey, new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions = [AdminPermissions.AccessAdminPanel],
        });
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));

        foreach (var action in new[] { "Variables", "OutputBindings" })
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"Admin/Workflows/Types/{id}/Designer/{action}")
            {
                Content = new StringContent("""{"revision":0,"activityId":"notify","variables":[],"bindings":{}}""", Encoding.UTF8, "application/json"),
            };
            await _fixture.AddAntiforgeryAsync(request);
            request.Headers.Add("PermissionsContext", permissionsContextKey);

            using var response = await _fixture.Context.Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Edit_ManageWorkflows_RendersDesignerWithTenantAwareConfig()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/Edit/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var host = Assert.Single(document.QuerySelectorAll("#workflow-designer"));
        var config = JsonNode.Parse(host.GetAttribute("data-config"));
        var tenantPrefix = $"/{_fixture.Context.TenantName}/";

        Assert.Equal(id, config["workflowTypeId"].GetValue<long>());
        Assert.False(config["readOnly"].GetValue<bool>());
        Assert.Equal($"{tenantPrefix}Admin/Workflows/Types/{id}/Designer/Definition", config["urls"]["definition"].GetValue<string>());
        Assert.Equal($"{tenantPrefix}Admin/Workflows/Types/{id}/Designer/Editor", config["urls"]["editor"].GetValue<string>());
        Assert.Equal($"{tenantPrefix}Admin/Workflows/Types/{id}/Designer/Versions", config["urls"]["versions"].GetValue<string>());
        Assert.Equal($"{tenantPrefix}Admin/Workflows/Types/Version/{id}", config["versionPageUrl"].GetValue<string>());
        Assert.Equal("designer", config["mode"].GetValue<string>());
        Assert.False(string.IsNullOrEmpty(config["translations"]["Undo"].GetValue<string>()));
        Assert.NotNull(document.QuerySelector("input[name='__RequestVerificationToken']"));
        Assert.Contains(document.QuerySelectorAll("script[src]"), x => x.GetAttribute("src").Contains("workflows-designer") && x.GetAttribute("type") == "module");
        Assert.DoesNotContain(document.QuerySelectorAll("script[src]"), x => x.GetAttribute("src").Contains("workflow-editor") || x.GetAttribute("src").Contains("jsplumb"));
        Assert.Null(config["initialActivityId"]);

        // The designer compares the signed-in user with the draft's last editor to show the draft banner, so
        // both are the same claim (null values are left out of the config).
        using var saved = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Save", new
        {
            revision = 0,
            nodes = new[] { new { id = "start", x = 0, y = 0, isStart = true } },
            transitions = Array.Empty<object>(),
            removedActivityIds = Array.Empty<string>(),
        });
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var definition = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        Assert.Equal(config["currentUserId"]?.GetValue<string>(), definition["draftModifiedByUserId"]?.GetValue<string>());
    }

    [Fact]
    public async Task Edit_ActivityIdQuery_OpensThatActivity()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/Edit/{id}?activityId=notify", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var config = JsonNode.Parse(Assert.Single(document.QuerySelectorAll("#workflow-designer")).GetAttribute("data-config"));
        Assert.Equal("notify", config["initialActivityId"].GetValue<string>());
    }

    [Theory]
    [InlineData("Designer/Index", "")]
    [InlineData("Designer/Index?activityId=notify", "?activityId=notify")]
    [InlineData("Activity/notify/Edit", "?activityId=notify")]
    [InlineData("Activity/NotifyTask/Add", "")]
    public async Task OldUrls_DesignerAndActivityPages_RedirectToTheDesigner(string path, string expectedQuery)
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/{id}/{path}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/{_fixture.Context.TenantName}/Admin/Workflows/Types/Edit/{id}{expectedQuery}", response.Headers.Location?.OriginalString);
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
    public async Task EditorPost_SetVariableTask_SavesTheVariableAndValidatesLiquid()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("set", "SetVariableTask"));

        using (var editor = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/{id}/Designer/Editor?activityId=set", TestContext.Current.CancellationToken))
        {
            var content = new HtmlParser().ParseDocument((await ReadJsonAsync(editor))["content"].GetValue<string>());

            // The designer fills this datalist with the declared variables.
            Assert.Equal("wfd-variables", content.QuerySelector("input[name='SetVariableTask.VariableName']")?.GetAttribute("list"));
        }

        using (var invalid = await PostFormAsync($"Admin/Workflows/Types/{id}/Designer/Editor?activityId=set&revision=0", new Dictionary<string, string>
        {
            ["SetVariableTask.VariableName"] = "greeting",
            ["SetVariableTask.Syntax"] = "Liquid",
            ["SetVariableTask.LiquidValue"] = "{{ 'Hello' ",
        }))
        {
            var json = await ReadJsonAsync(invalid);
            Assert.False(json["valid"].GetValue<bool>());
            Assert.Contains("Liquid", new HtmlParser().ParseDocument(json["content"].GetValue<string>()).QuerySelector(".field-validation-error")?.TextContent);
        }

        using var valid = await PostFormAsync($"Admin/Workflows/Types/{id}/Designer/Editor?activityId=set&revision=0", new Dictionary<string, string>
        {
            ["SetVariableTask.VariableName"] = " greeting ",
            ["SetVariableTask.Syntax"] = "Liquid",
            ["SetVariableTask.LiquidValue"] = "{{ 'Hello' }}",
        });

        Assert.True((await ReadJsonAsync(valid))["valid"].GetValue<bool>());

        await _fixture.Context.UsingTenantScopeAsync(async scope =>
        {
            var draft = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeDraftManager>().GetAsync(workflowTypeId);
            var properties = Assert.Single(draft.Activities).Properties;
            Assert.Equal("greeting", properties["VariableName"].GetValue<string>());
        });
    }

    [Fact]
    public async Task Variables_ValidDeclarations_SavesThemIntoTheDraft()
    {
        var (id, _) = await CreateWorkflowTypeAsync(
            Activity("start", "HttpRequestEvent", isStart: true),
            Activity("set", "SetVariableTask", properties: new JsonObject { ["VariableName"] = "count" }));

        var before = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");

        Assert.Empty(before["variables"].AsArray());
        var types = before["variableTypes"].AsArray().ToDictionary(x => x["name"].GetValue<string>(), x => x["editor"].GetValue<string>());
        Assert.Equal("number", types["number"]);
        Assert.Equal("json", types["object"]);
        Assert.Contains(before["issues"].AsArray(), x => x["code"].GetValue<string>() == "UndeclaredVariable" && x["activityId"].GetValue<string>() == "set");

        using var saved = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Variables", new
        {
            revision = 0,
            variables = new object[] { new { name = " count ", typeName = "number", defaultValue = 1, description = "Items seen" } },
        });

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var json = await ReadJsonAsync(saved);
        Assert.Equal(1, json["revision"].GetValue<int>());
        Assert.Equal("count", Assert.Single(json["variables"].AsArray())["name"].GetValue<string>());
        Assert.DoesNotContain(json["issues"].AsArray(), x => x["code"].GetValue<string>() == "UndeclaredVariable");

        using (var stale = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Variables", new { revision = 0, variables = Array.Empty<object>() }))
        {
            Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        }

        var after = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        var variable = Assert.Single(after["variables"].AsArray());
        Assert.Equal("number", variable["typeName"].GetValue<string>());
        Assert.Equal(1, variable["defaultValue"].GetValue<int>());
        Assert.Equal("Items seen", variable["description"].GetValue<string>());
    }

    [Fact]
    public async Task Variables_InvalidDeclarations_ReturnsVariableErrorsAndKeepsTheDraft()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));

        using var rejected = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Variables", new
        {
            revision = 0,
            variables = new object[]
            {
                new { name = "count", typeName = "number", defaultValue = "many" },
                new { name = "Count", typeName = "string" },
                new { name = "x", typeName = "unknown" },
            },
        });

        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Equal("application/problem+json", rejected.Content.Headers.ContentType?.MediaType);
        var errors = (await ReadJsonAsync(rejected))["variableErrors"].AsArray();
        Assert.Equal([0, 1, 2], errors.Select(x => x["index"].GetValue<int>()));

        await _fixture.Context.UsingTenantScopeAsync(async scope =>
            Assert.Null(await scope.ServiceProvider.GetRequiredService<IWorkflowTypeDraftManager>().GetAsync(workflowTypeId)));
    }

    [Fact]
    public async Task OutputBindings_HttpRequestOutputs_SavesTheBindingsAndReportsProblems()
    {
        var (id, _) = await CreateWorkflowTypeAsync(
            [Activity("start", "HttpRequestEvent", isStart: true), Activity("request", "HttpRequestTask")],
            [Transition("start", "Done", "request")]);

        var definition = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        var outputs = definition["nodes"][1]["outputs"].AsArray().ToDictionary(x => x["name"].GetValue<string>(), x => x["typeName"].GetValue<string>());
        Assert.Equal("number", outputs["StatusCode"]);
        Assert.Empty(definition["nodes"][1]["outputBindings"].AsObject());

        using (var declared = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Variables", new
        {
            revision = 0,
            variables = new object[] { new { name = "flag", typeName = "boolean" }, new { name = "body", typeName = "string" } },
        }))
        {
            Assert.Equal(HttpStatusCode.OK, declared.StatusCode);
        }

        using var bound = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/OutputBindings", new
        {
            revision = 1,
            activityId = "request",
            bindings = new Dictionary<string, string> { ["Body"] = "body", ["StatusCode"] = "flag", ["Response"] = "missing" },
        });

        Assert.Equal(HttpStatusCode.OK, bound.StatusCode);
        var json = await ReadJsonAsync(bound);
        Assert.Equal(2, json["revision"].GetValue<int>());
        Assert.Equal("flag", json["node"]["outputBindings"]["StatusCode"].GetValue<string>());
        var codes = json["issues"].AsArray().Select(x => x["code"].GetValue<string>()).Order().ToArray();
        Assert.Equal(["OutputTypeMismatch", "UndeclaredVariable"], codes);

        using var unknown = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/OutputBindings", new { revision = 2, activityId = "missing", bindings = new Dictionary<string, string>() });

        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task Instance_DeclaredVariables_ReturnsTheirStoredValues()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));

        using (var declared = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Variables", new
        {
            revision = 0,
            variables = new object[] { new { name = "greeting", typeName = "string" }, new { name = "count", typeName = "number" } },
        }))
        {
            Assert.Equal(HttpStatusCode.OK, declared.StatusCode);
        }

        using (var published = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Publish", new { revision = 1 }))
        {
            Assert.Equal(HttpStatusCode.OK, published.StatusCode);
        }

        var workflow = new Workflow
        {
            WorkflowId = Guid.NewGuid().ToString("n"),
            WorkflowTypeId = workflowTypeId,
            Status = WorkflowStatus.Finished,
            CreatedUtc = DateTime.UtcNow,
            State = new JsonObject
            {
                ["Properties"] = new JsonObject { ["greeting"] = "hello", ["internal"] = "not declared" },
            },
        };
        await _fixture.Context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IWorkflowStore>().SaveAsync(workflow));

        var json = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Instance?instanceId={workflow.Id}");

        Assert.Equal(["greeting", "count"], json["variables"].AsArray().Select(x => x["name"].GetValue<string>()));
        var value = Assert.Single(json["instance"]["variableValues"].AsObject());
        Assert.Equal("greeting", value.Key);
        Assert.Equal("hello", value.Value.GetValue<string>());
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
    public async Task RenderedShapes_JsonEndpoints_AreNotWrappedInTheLayout()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"), Activity("start", "HttpRequestEvent", isStart: true));

        // The first shape rendered in a request is the one that would be rendered as a main page.
        var definition = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        AssertFragment(definition["nodes"][0]["designHtml"].GetValue<string>());

        var library = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Library");
        AssertFragment(library["categories"][0]["activities"][0]["thumbnailHtml"].GetValue<string>());

        using var response = await PostFormAsync($"Admin/Workflows/Types/{id}/Designer/Editor?activityId=notify&revision=0", new Dictionary<string, string>
        {
            ["IActivity.ActivityMetadata.Title"] = string.Empty,
            ["NotifyTask.NotificationType"] = "Success",
            ["NotifyTask.Message"] = "Hello",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var designHtml = (await ReadJsonAsync(response))["node"]["designHtml"].GetValue<string>();
        AssertFragment(designHtml);
        Assert.Contains("Hello", designHtml);

        static void AssertFragment(string html)
        {
            Assert.False(string.IsNullOrWhiteSpace(html));
            Assert.DoesNotContain("<html", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<title", html, StringComparison.OrdinalIgnoreCase);
        }
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

    [Fact]
    public async Task Instance_BlockedInstance_ReturnsTheLiveGraphWithItsBlockingActivities()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true, x: 10), Activity("notify", "NotifyTask", x: 300));

        // The instance runs on the live type, so a draft change doesn't show.
        using (var saved = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Save", new
        {
            revision = 0,
            nodes = new[] { new { id = "start", x = 99, y = 0, isStart = true }, new { id = "notify", x = 300, y = 0, isStart = false } },
            transitions = Array.Empty<object>(),
            removedActivityIds = Array.Empty<string>(),
        }))
        {
            Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        }

        var instanceId = await _fixture.CreateInstanceAsync(workflowTypeId, WorkflowStatus.Halted, "notify");

        var json = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Instance?instanceId={instanceId}");

        Assert.False(json["hasDraft"].GetValue<bool>());
        Assert.Equal(10, json["nodes"][0]["x"].GetValue<int>());
        Assert.False(string.IsNullOrWhiteSpace(json["nodes"][1]["designHtml"].GetValue<string>()));
        var instance = json["instance"];
        Assert.Equal(instanceId, instance["id"].GetValue<long>());
        Assert.Equal("Halted", instance["status"].GetValue<string>());
        Assert.Equal(["notify"], instance["blockingActivityIds"].AsArray().Select(x => x.GetValue<string>()));
    }

    [Fact]
    public async Task Instance_OfAnotherWorkflowType_ReturnsNotFoundProblem()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));
        var (_, otherWorkflowTypeId) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));
        var instanceId = await _fixture.CreateInstanceAsync(otherWorkflowTypeId, WorkflowStatus.Halted, "start");

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/{id}/Designer/Instance?instanceId={instanceId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Details_Instance_MountsTheReadOnlyDesigner()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("start", "HttpRequestEvent", isStart: true));
        var instanceId = await _fixture.CreateInstanceAsync(workflowTypeId, WorkflowStatus.Halted, "start");

        using var response = await _fixture.Context.Client.GetAsync($"Admin/OrchardCore.Workflows/Workflow/Details/{instanceId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var config = JsonNode.Parse(Assert.Single(document.QuerySelectorAll("#workflow-designer")).GetAttribute("data-config"));
        Assert.True(config["readOnly"].GetValue<bool>());
        Assert.EndsWith($"Admin/Workflows/Types/{id}/Designer/Instance?instanceId={instanceId}", config["urls"]["definition"].GetValue<string>());
        Assert.Null(config["urls"]["save"]);

        // The State tab is kept, and the jsPlumb viewer and the stale Bootstrap 4 script are gone.
        Assert.NotNull(document.QuerySelector("#state pre"));
        var scripts = document.QuerySelectorAll("script[src]").Select(x => x.GetAttribute("src")).ToList();
        Assert.Contains(scripts, x => x.Contains("workflows-designer"));
        Assert.DoesNotContain(scripts, x => x.Contains("workflow-viewer") || x.Contains("jsplumb"));
        Assert.DoesNotContain(scripts, x => x.Contains("bootstrap") && x.Contains("4."));
    }

    [Fact]
    public async Task Versions_AfterPublishing_ListsTheVersionsWithTheirInstances()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var firstVersionId = (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();
        await _fixture.CreatePinnedInstanceAsync(workflowTypeId, firstVersionId, WorkflowStatus.Halted, "notify");

        var published = await PublishWithNotifyAtAsync(id, x: 40);

        Assert.Equal(2, published["version"]["version"].GetValue<int>());
        Assert.True(published["version"]["isPublished"].GetValue<bool>());

        var json = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Versions");
        var versions = json["versions"].AsArray();

        Assert.Equal(new[] { 2, 1 }, versions.Select(version => version["version"].GetValue<int>()));
        Assert.Equal(new[] { true, false }, versions.Select(version => version["isPublished"].GetValue<bool>()));
        Assert.Equal(new[] { 0, 1 }, versions.Select(version => version["instanceCount"].GetValue<int>()));
        Assert.Equal(firstVersionId, versions[1]["versionId"].GetValue<string>());
        Assert.Null(json["draft"]);

        var definition = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        Assert.Equal(2, definition["publishedVersion"]["version"].GetValue<int>());
    }

    [Fact]
    public async Task Version_EarlierVersion_ReturnsItsGraph()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var firstVersionId = (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();
        await PublishWithNotifyAtAsync(id, x: 40);

        var json = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Version?versionId={firstVersionId}");

        Assert.Equal(0, json["nodes"][0]["x"].GetValue<int>());
        Assert.Equal(1, json["version"]["version"].GetValue<int>());
        Assert.Equal(2, json["publishedVersion"]["version"].GetValue<int>());
        Assert.False(json["hasDraft"].GetValue<bool>());

        using var unknown = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/{id}/Designer/Version?versionId=unknown", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task Version_OfAnotherWorkflowType_ReturnsNotFoundProblem()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var (otherId, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var otherVersionId = (await GetJsonAsync($"Admin/Workflows/Types/{otherId}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/{id}/Designer/Version?versionId={otherVersionId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Compare_VersionWithDraft_ReturnsBothGraphsAndTheChanges()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var versionId = (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();

        await SaveNotifyAtAsync(id, x: 40, revision: 0);

        string addedId;
        using (var added = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/AddActivity", new { revision = 1, name = "NotifyTask", x = 0, y = 200 }))
        {
            addedId = (await ReadJsonAsync(added))["node"]["id"].GetValue<string>();
        }

        var json = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Compare?from={versionId}&to=draft");

        Assert.Equal(1, json["from"]["version"]["version"].GetValue<int>());
        Assert.True(json["to"]["hasDraft"].GetValue<bool>());
        Assert.Single(json["from"]["nodes"].AsArray());
        Assert.Equal(2, json["to"]["nodes"].AsArray().Count);

        var changes = json["changes"];
        Assert.Equal([addedId], changes["addedActivityIds"].AsArray().Select(x => x.GetValue<string>()));
        Assert.Equal(["notify"], changes["movedActivityIds"].AsArray().Select(x => x.GetValue<string>()));
        Assert.Empty(changes["removedActivityIds"].AsArray());
        Assert.True(changes["hasChanges"].GetValue<bool>());
    }

    [Fact]
    public async Task Restore_EarlierVersion_CopiesItIntoTheDraft()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var firstVersionId = (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();
        await PublishWithNotifyAtAsync(id, x: 40);

        using (var restored = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Restore", new { revision = 0, versionId = firstVersionId }))
        {
            Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
            Assert.Equal(1, (await ReadJsonAsync(restored))["revision"].GetValue<int>());
        }

        var definition = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition");
        Assert.True(definition["hasDraft"].GetValue<bool>());
        Assert.Equal(0, definition["nodes"][0]["x"].GetValue<int>());

        // The draft moved on since revision 0.
        using var stale = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Restore", new { revision = 0, versionId = firstVersionId });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
    }

    [Fact]
    public async Task Instance_PinnedToAnEarlierVersion_ShowsThatVersion()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var firstVersionId = (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();
        await PublishWithNotifyAtAsync(id, x: 40);
        var instanceId = await _fixture.CreatePinnedInstanceAsync(workflowTypeId, firstVersionId, WorkflowStatus.Halted, "notify");

        var json = await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Instance?instanceId={instanceId}");

        Assert.Equal(0, json["nodes"][0]["x"].GetValue<int>());
        Assert.Equal(1, json["version"]["version"].GetValue<int>());
        Assert.Equal(2, json["publishedVersion"]["version"].GetValue<int>());
    }

    [Fact]
    public async Task InstancesList_PinnedAndUnpinnedInstances_ShowsTheVersionOfEach()
    {
        var (id, workflowTypeId) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var firstVersionId = (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();
        await PublishWithNotifyAtAsync(id, x: 40);
        var pinnedId = await _fixture.CreatePinnedInstanceAsync(workflowTypeId, firstVersionId, WorkflowStatus.Halted, "notify");
        var unpinnedId = await _fixture.CreateInstanceAsync(workflowTypeId, WorkflowStatus.Halted, "notify");

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/{id}/Instances/Index", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var pinned = document.QuerySelector($"#itemIds-{pinnedId}").Closest("li");
        var unpinned = document.QuerySelector($"#itemIds-{unpinnedId}").Closest("li");
        Assert.Equal("Version 1", pinned.QuerySelector("[data-cy=instance-version]").TextContent.Trim());
        Assert.Null(unpinned.QuerySelector("[data-cy=instance-version]"));
    }

    private async Task SaveNotifyAtAsync(long id, int x, int revision)
    {
        using var saved = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Save", new
        {
            revision,
            nodes = new[] { new { id = "notify", x, y = 0, isStart = false } },
            transitions = Array.Empty<object>(),
            removedActivityIds = Array.Empty<string>(),
        });

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
    }

    // Moves the "notify" activity in a new draft and publishes it, which creates the next version.
    private async Task<JsonNode> PublishWithNotifyAtAsync(long id, int x)
    {
        await SaveNotifyAtAsync(id, x, revision: 0);

        using var published = await PostJsonAsync($"Admin/Workflows/Types/{id}/Designer/Publish", new { revision = 1 });
        Assert.Equal(HttpStatusCode.OK, published.StatusCode);

        return await ReadJsonAsync(published);
    }

    [Fact]
    public async Task VersionPage_EarlierVersion_MountsTheReadOnlyDesigner()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var versionId = (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/Version/{id}?versionId={versionId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var config = JsonNode.Parse(Assert.Single(document.QuerySelectorAll("#workflow-designer")).GetAttribute("data-config"));
        Assert.Equal("version", config["mode"].GetValue<string>());
        Assert.True(config["readOnly"].GetValue<bool>());
        Assert.EndsWith($"Admin/Workflows/Types/{id}/Designer/Version?versionId={versionId}", config["urls"]["definition"].GetValue<string>());
        Assert.Null(config["urls"]["save"]);
        Assert.EndsWith($"Admin/Workflows/Types/Edit/{id}", config["designerUrl"].GetValue<string>());

        using var unknown = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/Version/{id}?versionId=unknown", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task CompareVersionsPage_VersionAndDraft_MountsTheComparison()
    {
        var (id, _) = await CreateWorkflowTypeAsync(Activity("notify", "NotifyTask"));
        var versionId = (await GetJsonAsync($"Admin/Workflows/Types/{id}/Designer/Definition"))["publishedVersion"]["versionId"].GetValue<string>();

        using var response = await _fixture.Context.Client.GetAsync($"Admin/Workflows/Types/CompareVersions/{id}?from={versionId}&to=draft", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var config = JsonNode.Parse(Assert.Single(document.QuerySelectorAll("#workflow-designer")).GetAttribute("data-config"));
        Assert.Equal("compare", config["mode"].GetValue<string>());
        Assert.EndsWith($"Admin/Workflows/Types/{id}/Designer/Compare?from={versionId}&to=draft", config["urls"]["compare"].GetValue<string>());
        Assert.Null(config["urls"]["definition"]);
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

    public Task<long> CreateInstanceAsync(string workflowTypeId, WorkflowStatus status, params string[] blockingActivityIds)
        => CreatePinnedInstanceAsync(workflowTypeId, null, status, blockingActivityIds);

    public async Task<long> CreatePinnedInstanceAsync(string workflowTypeId, string versionId, WorkflowStatus status, params string[] blockingActivityIds)
    {
        var workflow = new Workflow
        {
            WorkflowId = Guid.NewGuid().ToString("n"),
            WorkflowTypeId = workflowTypeId,
            WorkflowTypeVersionId = versionId,
            Status = status,
            CreatedUtc = DateTime.UtcNow,
            BlockingActivities = blockingActivityIds.Select(activityId => new BlockingActivity { ActivityId = activityId, Name = activityId }).ToList(),
        };

        await Context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IWorkflowStore>().SaveAsync(workflow));

        return workflow.Id;
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
