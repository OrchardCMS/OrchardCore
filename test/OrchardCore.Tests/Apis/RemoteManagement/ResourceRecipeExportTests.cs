using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Json;
using OrchardCore.Layers.Deployment;
using OrchardCore.Placements.Deployment;
using OrchardCore.Queries.Deployment;
using OrchardCore.Recipes.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Roles.Deployment;
using OrchardCore.Security;
using OrchardCore.Shortcodes.Deployment;
using OrchardCore.Templates.Deployment;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ResourceRecipeExportTests
{
    private static readonly string[] _features =
    [
        "OrchardCore.Deployment",
        "OrchardCore.Layers",
        "OrchardCore.Placements",
        "OrchardCore.Queries.Sql",
        "OrchardCore.Shortcodes.Templates",
        "OrchardCore.Templates",
    ];

    private static readonly string[] _exportOperations =
    [
        "ApiExportQuery",
        "ApiExportRole",
        "ApiExportTemplate",
        "ApiExportLayer",
        "ApiExportPlacement",
        "ApiExportShortcodeTemplate",
    ];

    private static readonly ResourceCase[] _resources =
    [
        new("Queries", () => new AllQueriesDeploymentStep(), "api/queries",
            name => $"api/queries/named/{name}", name => $"api/queries/named/{name}:export",
            name => new JsonObject
            {
                ["name"] = name,
                ["source"] = "Sql",
                ["returnContentItems"] = false,
                ["properties"] = new JsonObject { ["SqlQueryMetadata"] = new JsonObject { ["Template"] = "select DocumentId from ContentItemIndex" } },
            }),
        new("Roles", () => new AllRolesDeploymentStep(), "api/roles",
            name => $"api/roles/{name}", name => $"api/roles/{name}:export",
            name => new JsonObject { ["roleName"] = name, ["roleDescription"] = "Exported role", ["permissionNames"] = new JsonArray("ViewContent") }),
        new("Templates", () => new AllTemplatesDeploymentStep(), "api/templates",
            name => $"api/templates/{name}", name => $"api/templates/{name}:export",
            name => new JsonObject { ["name"] = name, ["description"] = "Exported template", ["content"] = "<b>{{ Model.Content }}</b>" }),
        new("Layers", () => new AllLayersDeploymentStep(), "api/layers",
            name => $"api/layers/by-name?name={name}", name => $"api/layers/by-name:export?name={name}",
            name => new JsonObject
            {
                ["name"] = name,
                ["description"] = "Exported layer",
                ["conditions"] = new JsonArray(new JsonObject
                {
                    ["name"] = "UrlCondition",
                    ["properties"] = new JsonObject { ["value"] = "/news", ["operation"] = "StringStartsWithOperator" },
                }),
            }),
        new("Placements", () => new PlacementsDeploymentStep(), "api/placements",
            name => $"api/placements/by-shape?shapeType={name}", name => $"api/placements/by-shape:export?shapeType={name}",
            name => new JsonObject { ["shapeType"] = name, ["nodes"] = new JsonArray(new JsonObject { ["place"] = "Content:1" }) }),
        new("ShortcodeTemplates", () => new AllShortcodeTemplatesDeploymentStep(), "api/shortcode-templates",
            name => $"api/shortcode-templates/by-name?name={name}", name => $"api/shortcode-templates/by-name:export?name={name}",
            name => new JsonObject { ["name"] = name, ["content"] = "<b>{{ Content }}</b>", ["hint"] = "Exported shortcode" }),
    ];

    [Fact]
    public async Task Export_DeletedResourceRestoredFromTheRecipe_MatchesTheOriginal()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var site = await CreateSiteAsync();

        foreach (var resource in _resources)
        {
            var name = "exportprobe" + resource.Step.ToLowerInvariant();
            var other = "otherprobe" + resource.Step.ToLowerInvariant();
            await PostAsync(site.Client, resource.CreatePath, resource.Body(name), cancellationToken);
            await PostAsync(site.Client, resource.CreatePath, resource.Body(other), cancellationToken);
            var original = await GetJsonAsync(site.Client, resource.ReadPath(name), cancellationToken);

            // The recipe holds this resource alone, as the all-items step describes it.
            var recipe = (await GetJsonAsync(site.Client, resource.ExportPath(name), cancellationToken)).AsObject();
            var step = Assert.Single(recipe["steps"].AsArray()).AsObject();
            Assert.Equal(resource.Step, step["name"].GetValue<string>(), ignoreCase: true);
            Assert.Equal(1, Count(step[resource.Step]));
            Assert.Null(Entry(step[resource.Step], other));
            JsonObject everything = null;
            await site.UsingTenantScopeAsync(async scope => everything = await scope.ServiceProvider.GetRequiredService<IDeploymentManager>()
                .ExportRecipeAsync(resource.AllItems(), "Everything", new ClaimsPrincipal()));
            var complete = everything["steps"].AsArray().First(candidate => string.Equals(candidate["name"].GetValue<string>(), resource.Step, StringComparison.OrdinalIgnoreCase));
            Assert.True(Count(complete[resource.Step]) >= 2, resource.Step);
            Assert.True(JsonNode.DeepEquals(Entry(complete[resource.Step], name), Entry(step[resource.Step], name)), resource.Step);

            using (var deleted = await site.Client.DeleteAsync(resource.ReadPath(name), cancellationToken))
            {
                Assert.True(deleted.IsSuccessStatusCode, await deleted.Content.ReadAsStringAsync(cancellationToken));
            }

            using (var missing = await site.Client.GetAsync(resource.ReadPath(name), cancellationToken))
            {
                Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
            }

            await ExecuteRecipeAsync(site, recipe);

            var restored = await GetJsonAsync(site.Client, resource.ReadPath(name), cancellationToken);
            Assert.True(JsonNode.DeepEquals(original, restored), $"{resource.Step}: {original.ToJsonString()} became {restored.ToJsonString()}");
        }
    }

    [Fact]
    public async Task Export_MissingResource_ReturnsNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var site = await CreateSiteAsync();

        foreach (var resource in _resources)
        {
            using var response = await site.Client.GetAsync(resource.ExportPath("missingprobe"), cancellationToken);

            Assert.True(response.StatusCode == HttpStatusCode.NotFound, $"{resource.Step} answered {(int)response.StatusCode}.");
        }
    }

    [Fact]
    public async Task Export_Endpoints_RequireTheExportAndResourcePermissions()
    {
        using var site = await CreateSiteAsync();

        var endpoints = await GetExportEndpointsAsync(site);

        Assert.Equal(_exportOperations.Order(), endpoints.Keys.Order());
        foreach (var (operation, permission) in new Dictionary<string, string[]>
        {
            ["ApiExportQuery"] = ["ManageQueries"],
            ["ApiExportRole"] = ["ManageRoles"],
            ["ApiExportTemplate"] = ["ManageTemplates", "ManageLiquidTemplates"],
            ["ApiExportLayer"] = ["ManageLayers"],
            ["ApiExportPlacement"] = ["ManagePlacements"],
            ["ApiExportShortcodeTemplate"] = ["ManageShortcodeTemplates"],
        })
        {
            var names = endpoints[operation].Metadata.GetOrderedMetadata<AuthorizationPolicy>()
                .SelectMany(policy => policy.Requirements).OfType<PermissionRequirement>().Select(requirement => requirement.Permission.Name).ToArray();
            Assert.Contains("AccessRemoteManagement", names);
            Assert.Contains("Export", names);
            Assert.All(permission, expected => Assert.Contains(expected, names));
        }
    }

    [Fact]
    public async Task Export_DeploymentDisabled_DoesNotMapTheRoutes()
    {
        using var site = await CreateSiteAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await features.GetAvailableFeaturesAsync();
            await features.DisableFeaturesAsync(available.Where(feature => feature.Id == "OrchardCore.Deployment"), force: true);
        });
        await site.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        Assert.Empty(await GetExportEndpointsAsync(site));
    }

    [Fact]
    public async Task AllItemsSteps_SavedWithoutNames_KeepExportingEverything()
    {
        using var site = await CreateSiteAsync();
        foreach (var resource in _resources.Where(resource => resource.Step is "Queries" or "Layers"))
        {
            await PostAsync(site.Client, resource.CreatePath, resource.Body("firstprobe"), TestContext.Current.CancellationToken);
            await PostAsync(site.Client, resource.CreatePath, resource.Body("secondprobe"), TestContext.Current.CancellationToken);
        }

        await site.UsingTenantScopeAsync(async scope =>
        {
            var options = scope.ServiceProvider.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>().Value.SerializerOptions;
            var plan = new DeploymentPlan { Name = "Saved" };
            plan.DeploymentSteps.AddRange(_resources.Select(resource => resource.AllItems()));

            // The document of a plan saved before the filter existed is the same as one without a filter.
            var saved = JsonSerializer.Serialize(plan, options);
            Assert.DoesNotContain("\"Names\"", saved, StringComparison.Ordinal);

            var restored = JsonSerializer.Deserialize<DeploymentPlan>(saved, options);
            Assert.Equal(plan.DeploymentSteps.Select(step => step.GetType()), restored.DeploymentSteps.Select(step => step.GetType()));
            Assert.All(restored.DeploymentSteps, step => Assert.Null(step.GetType().GetProperty("Names").GetValue(step)));

            var result = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor());
            await scope.ServiceProvider.GetRequiredService<IDeploymentManager>().ExecuteDeploymentPlanAsync(restored, result);

            foreach (var key in new[] { "Queries", "Layers" })
            {
                var entries = result.Steps.Single(step => step["name"].GetValue<string>() == key)[key];
                Assert.NotNull(Entry(entries, "firstprobe"));
                Assert.NotNull(Entry(entries, "secondprobe"));
            }

            Assert.Contains(result.Steps, step => step.ContainsKey("LayerSettings"));
        });
    }

    [Fact]
    public async Task AllItemsSteps_SavedWithNames_RoundTripAndFilter()
    {
        using var site = await CreateSiteAsync();
        var resource = _resources.Single(resource => resource.Step == "Queries");
        await PostAsync(site.Client, resource.CreatePath, resource.Body("firstprobe"), TestContext.Current.CancellationToken);
        await PostAsync(site.Client, resource.CreatePath, resource.Body("secondprobe"), TestContext.Current.CancellationToken);

        await site.UsingTenantScopeAsync(async scope =>
        {
            var options = scope.ServiceProvider.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>().Value.SerializerOptions;
            var plan = new DeploymentPlan { Name = "Filtered" };
            plan.DeploymentSteps.Add(new AllQueriesDeploymentStep { Names = ["secondprobe"] });

            var restored = JsonSerializer.Deserialize<DeploymentPlan>(JsonSerializer.Serialize(plan, options), options);
            var step = Assert.IsType<AllQueriesDeploymentStep>(Assert.Single(restored.DeploymentSteps));
            Assert.Equal(["secondprobe"], step.Names);

            var result = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor());
            await scope.ServiceProvider.GetRequiredService<IDeploymentManager>().ExecuteDeploymentPlanAsync(restored, result);

            var exported = Assert.Single(result.Steps)["Queries"];
            Assert.Equal(1, Count(exported));
            Assert.NotNull(Entry(exported, "secondprobe"));

            // An empty selection isn't a filter, as in a step without names.
            var all = new DeploymentPlan { Name = "Empty" };
            all.DeploymentSteps.Add(new AllQueriesDeploymentStep { Names = [] });
            var unfiltered = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor());
            await scope.ServiceProvider.GetRequiredService<IDeploymentManager>().ExecuteDeploymentPlanAsync(all, unfiltered);
            Assert.NotNull(Entry(Assert.Single(unfiltered.Steps)["Queries"], "firstprobe"));
            Assert.NotNull(Entry(Assert.Single(unfiltered.Steps)["Queries"], "secondprobe"));
        });
    }

    [Fact]
    public async Task AllTemplatesStep_ExportAsFilesWithNames_WritesOnlyTheSelectedFiles()
    {
        using var site = await CreateSiteAsync();
        var resource = _resources.Single(resource => resource.Step == "Templates");
        await PostAsync(site.Client, resource.CreatePath, resource.Body("First"), TestContext.Current.CancellationToken);
        await PostAsync(site.Client, resource.CreatePath, resource.Body("Second"), TestContext.Current.CancellationToken);

        await site.UsingTenantScopeAsync(async scope =>
        {
            var files = new Mock<IFileBuilder>();
            var plan = new DeploymentPlan { Name = "Files" };
            plan.DeploymentSteps.Add(new AllTemplatesDeploymentStep { ExportAsFiles = true, Names = ["Second"] });
            var result = new DeploymentPlanResult(files.Object, new RecipeDescriptor());

            await scope.ServiceProvider.GetRequiredService<IDeploymentManager>().ExecuteDeploymentPlanAsync(plan, result);

            Assert.Equal(1, Count(Assert.Single(result.Steps)["Templates"]));
            files.Verify(builder => builder.SetFileAsync("Templates/Second.liquid", It.IsAny<Stream>()), Times.Once);
            files.Verify(builder => builder.SetFileAsync("Templates/First.liquid", It.IsAny<Stream>()), Times.Never);
        });
    }

    [Theory]
    [InlineData(null, "name", true)]
    [InlineData(new string[0], "name", true)]
    [InlineData(new[] { "name" }, "name", true)]
    [InlineData(new[] { "Name" }, "name", false)]
    [InlineData(new[] { "other" }, "name", false)]
    public void IsSelected_Names_FiltersOnlyWhenThereAreNames(string[] names, string name, bool expected)
    {
        Assert.Equal(expected, DeploymentSelection.IsSelected(names, name));
    }

    private static async Task<SiteContext> CreateSiteAsync()
    {
        var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await features.GetAvailableFeaturesAsync();
            await features.EnableFeaturesAsync(available.Where(feature => _features.Contains(feature.Id, StringComparer.Ordinal)), force: true);
        });
        var credentials = RemoteManagementClientCredentials.Generate();
        Assert.True(SiteContext.ShellHost.TryGetSettings(site.TenantName, out var settings));
        Assert.True(await RemoteManagementProvisioning.ConfigureAsync(SiteContext.ShellHost, settings, credentials));

        var cancellationToken = TestContext.Current.CancellationToken;
        using var tokenResponse = await site.Client.PostAsync("connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = credentials.ClientId,
            ["client_secret"] = credentials.ClientSecret,
            ["scope"] = RemoteManagementConstants.ManagementScope,
        }), cancellationToken);
        var payload = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        Assert.True(tokenResponse.IsSuccessStatusCode, payload);
        using var token = JsonDocument.Parse(payload);
        site.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.RootElement.GetProperty("access_token").GetString());

        return site;
    }

    private static async Task<Dictionary<string, RouteEndpoint>> GetExportEndpointsAsync(SiteContext site)
    {
        // Loading a named endpoint initializes the endpoints of the enabled features.
        using var warmup = await site.Client.GetAsync("__eptnametest?name=ApiListContentItems", TestContext.Current.CancellationToken);
        warmup.EnsureSuccessStatusCode();

        Dictionary<string, RouteEndpoint> endpoints = null;
        await site.UsingTenantScopeAsync(scope =>
        {
            endpoints = scope.ServiceProvider.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
                .Where(endpoint => _exportOperations.Contains(endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName))
                .ToDictionary(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>().EndpointName);

            return Task.CompletedTask;
        });

        return endpoints;
    }

    private static async Task PostAsync(HttpClient client, string path, JsonObject body, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(path, body, cancellationToken);
        Assert.True(response.IsSuccessStatusCode, $"{path}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync(cancellationToken)}");
    }

    private static async Task<JsonNode> GetJsonAsync(HttpClient client, string path, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(path, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.True(response.IsSuccessStatusCode, $"{path}: {(int)response.StatusCode} {payload}");

        return JsonNode.Parse(payload);
    }

    private static async Task ExecuteRecipeAsync(SiteContext site, JsonObject recipe)
    {
        var folder = Directory.CreateTempSubdirectory("resource-recipe-");
        try
        {
            await File.WriteAllTextAsync(Path.Combine(folder.FullName, "Recipe.json"), recipe.ToJsonString(), TestContext.Current.CancellationToken);
            using var files = new PhysicalFileProvider(folder.FullName);
            await site.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IDeploymentManager>().ImportDeploymentPackageAsync(files));
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    private static int Count(JsonNode entries) => entries is JsonObject entriesObject ? entriesObject.Count : entries.AsArray().Count;

    /// <summary>
    /// Finds an entry of a recipe step, whose items are keyed by name, or carry their name.
    /// </summary>
    private static JsonNode Entry(JsonNode entries, string name) => entries is JsonObject entriesObject
        ? entriesObject.FirstOrDefault(entry => string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase)).Value
        : entries.AsArray().FirstOrDefault(entry => string.Equals(entry["Name"]?.GetValue<string>(), name, StringComparison.OrdinalIgnoreCase));

    private sealed record ResourceCase(
        string Step,
        Func<DeploymentStep> AllItems,
        string CreatePath,
        Func<string, string> ReadPath,
        Func<string, string> ExportPath,
        Func<string, JsonObject> Body);
}
