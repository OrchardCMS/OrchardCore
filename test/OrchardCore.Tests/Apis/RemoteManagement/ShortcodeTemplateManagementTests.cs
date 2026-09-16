using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Shortcodes.Endpoints.Management;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Recipes;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;
using OrchardCore.Tests.Apis.Context;
using ShortcodeAdminController = OrchardCore.Shortcodes.Controllers.AdminController;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ShortcodeTemplateManagementTests
{
    [Fact]
    public async Task ApiRoundTrip_ValidatesSanitizesAndConvergesAcrossScopes()
    {
        using var context = await CreateContextAsync();
        var definition = new ShortcodeTemplateDefinition
        {
            Name = "ApiProbe", Content = "<b>{{ Content }}</b>", Hint = "Original",
            Usage = "<strong>Safe</strong><script>alert(1)</script>", DefaultValue = "[apiprobe][/apiprobe]",
            Categories = ["Test"],
        };
        for (var retry = 0; retry < 2; retry++)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var manager = scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>();
                var result = Assert.IsType<Created<ShortcodeTemplateDefinition>>(await ShortcodeTemplateManagementEndpoints.CreateAsync(
                    new DefaultHttpContext(), Authorize(true), manager, definition)).Value;
                Assert.Equal("apiprobe", result.Name);
                Assert.Equal(definition.Content, result.Content);
                Assert.DoesNotContain("<script", result.Usage, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("<strong>Safe</strong>", result.Usage);
                Assert.Equal(definition.Categories, result.Categories);
            });
        }
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>();
            var conflict = await ShortcodeTemplateManagementEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(true), manager,
                new() { Name = "APIPROBE", Content = "different" });
            Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(conflict).StatusCode);
            var invalid = new ShortcodeTemplateDefinition { Name = "apiprobe", Content = "{% if %}" };
            var validation = Assert.IsType<Ok<ShortcodeTemplateValidationResponse>>(await ShortcodeTemplateManagementEndpoints.ValidateAsync(
                new DefaultHttpContext(), Authorize(true), manager, invalid)).Value;
            Assert.False(validation.IsValid);
            Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await ShortcodeTemplateManagementEndpoints.UpdateAsync(
                new DefaultHttpContext(), Authorize(true), manager, "apiprobe", invalid)).StatusCode);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>();
            var stored = Assert.IsType<Ok<ShortcodeTemplateDefinition>>(await ShortcodeTemplateManagementEndpoints.GetAsync(
                new DefaultHttpContext(), Authorize(true), manager, "ApiProbe")).Value;
            Assert.Equal("Original", stored.Hint);
            var updated = Assert.IsType<Ok<ShortcodeTemplateDefinition>>(await ShortcodeTemplateManagementEndpoints.UpdateAsync(
                new DefaultHttpContext(), Authorize(true), manager, "apiprobe", new() { Name = "apiprobe", Content = "Changed" })).Value;
            Assert.Equal("Changed", updated.Content);
            Assert.Null(updated.Hint);
            Assert.Empty(updated.Categories);
        });
        for (var retry = 0; retry < 2; retry++)
        {
            await context.UsingTenantScopeAsync(async scope => Assert.IsType<NoContent>(await ShortcodeTemplateManagementEndpoints.DeleteAsync(
                new DefaultHttpContext(), Authorize(true), scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>(), "APIPROBE")));
        }
    }

    [Fact]
    public async Task AdminRename_UsesSharedValidationAndPreservesConflictingTemplates()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var controller = CreateController(scope.ServiceProvider);
            Assert.IsType<RedirectToActionResult>(await controller.CreatePost(new()
            {
                Name = "AdminProbe", Content = "Original", SelectedCategories = "[\"Test\"]", Usage = "<script>alert(1)</script><b>Safe</b>",
            }, "Save"));
            Assert.IsType<ViewResult>(await controller.CreatePost(new() { Name = "adminprobe", Content = "Original", SelectedCategories = "[]" }, "Save"));
            Assert.False(controller.ModelState.IsValid);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var controller = CreateController(scope.ServiceProvider);
            Assert.IsType<RedirectToActionResult>(await controller.Edit("adminprobe", new()
            {
                Name = "RenamedProbe", Content = "Renamed", Hint = "Edited", SelectedCategories = "[\"Other\"]",
            }, "Save"));
            var manager = scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>();
            Assert.Equal(ShortcodeTemplateMutationStatus.Saved, (await manager.SaveAsync("occupied", new() { Content = "Other" })).Status);
            Assert.IsType<ViewResult>(await controller.Edit("renamedprobe", new() { Name = "occupied", Content = "Wrong", SelectedCategories = "[]" }, "Save"));
            Assert.False(controller.ModelState.IsValid);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var document = await scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>().GetShortcodeTemplatesDocumentAsync();
            Assert.False(document.ShortcodeTemplates.ContainsKey("adminprobe"));
            Assert.Equal("Renamed", document.ShortcodeTemplates["renamedprobe"].Content);
            Assert.Equal("Edited", document.ShortcodeTemplates["renamedprobe"].Hint);
            Assert.Equal(["Other"], document.ShortcodeTemplates["renamedprobe"].Categories);
            Assert.Equal("Other", document.ShortcodeTemplates["occupied"].Content);
        });
    }

    [Fact]
    public async Task RecipeImport_UsesSharedSanitizationAndPreservesImportCompatibility()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>();
            var recipe = new ShortcodeTemplateStep(manager);
            // Recipes historically import definitions without editor Liquid syntax validation.
            await recipe.ExecuteAsync(new RecipeExecutionContext
            {
                Name = "ShortcodeTemplates",
                Step = JsonNode.Parse("""{"ShortcodeTemplates":{"Imported":{"Content":"{% if %}","Usage":"<script>alert(1)</script><b>Safe</b>"}}}""").AsObject(),
            });
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var imported = (await scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>().GetShortcodeTemplatesDocumentAsync()).ShortcodeTemplates["imported"];
            Assert.Equal("{% if %}", imported.Content);
            Assert.DoesNotContain("<script", imported.Usage, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<b>Safe</b>", imported.Usage);
        });
    }

    [Fact]
    public async Task Names_UseParserValidationAndInvariantCase()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>();
            foreach (var name in new[] { "", "bad name", "[nested]", "two][nodes", new string('a', 257) })
            {
                Assert.Equal(ShortcodeTemplateMutationStatus.Invalid, (await manager.SaveAsync(name, new() { Content = "Valid" })).Status);
            }
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                var result = await manager.SaveAsync("IDENTIFIER", new() { Content = "Valid", Categories = null });
                Assert.Equal("identifier", result.Name);
                Assert.Empty(result.Template.Categories);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        });
    }

    [Fact]
    public async Task DeniedOperations_DoNotResolveOrUseTheManager()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var auth = Authorize(false);
        IResult[] results =
        [
            await ShortcodeTemplateManagementEndpoints.ListAsync(http, auth, null, new()),
            await ShortcodeTemplateManagementEndpoints.GetAsync(http, auth, null, "name"),
            await ShortcodeTemplateManagementEndpoints.ValidateAsync(http, auth, null, new()),
            await ShortcodeTemplateManagementEndpoints.CreateAsync(http, auth, null, new()),
            await ShortcodeTemplateManagementEndpoints.UpdateAsync(http, auth, null, "name", new()),
            await ShortcodeTemplateManagementEndpoints.DeleteAsync(http, auth, null, "name"),
        ];
        Assert.All(results, result => Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode));
    }

    private static ShortcodeAdminController CreateController(IServiceProvider services)
    {
        var controller = ActivatorUtilities.CreateInstance<ShortcodeAdminController>(services, Authorize(true));
        controller.ControllerContext = new() { HttpContext = new DefaultHttpContext { RequestServices = services } };
        controller.Url = Mock.Of<IUrlHelper>();
        return controller;
    }

    private static async Task<SiteContext> CreateContextAsync()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = await manager.GetAvailableFeaturesAsync();
            await manager.EnableFeaturesAsync(features.Where(feature => feature.Id == "OrchardCore.Shortcodes.Templates"), force: true);
        });
        return context;
    }

    private static IAuthorizationService Authorize(bool success)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
            It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(success ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
