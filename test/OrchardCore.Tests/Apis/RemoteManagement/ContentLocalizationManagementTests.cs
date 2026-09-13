using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentLocalization.Endpoints;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Title.Models;
using LocalizationAdminController = OrchardCore.ContentLocalization.Controllers.AdminController;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ContentLocalizationManagementTests
{
    private static readonly ClaimsPrincipal s_user = new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "localizer")], "test"));

    [Fact]
    public async Task Localize_CloneRetryAndAdmin_PreserveSourceAndCreateDrafts()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var sourceId = await PrepareAsync(context);
        string translatedId = null;
        for (var retry = 0; retry < 2; retry++)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var services = scope.ServiceProvider;
                var result = Assert.IsType<Ok<LocalizeContentResponse>>(await ContentLocalizationEndpoints.LocalizeAsync(Http(services),
                    Authorize(), CreateService(services), sourceId, new() { Culture = "FR" })).Value;
                Assert.Equal(retry == 0, result.Created);
                Assert.Equal("fr", result.Item.Culture);
                Assert.False(result.Item.Published);
                Assert.True(result.Item.Latest);
                Assert.NotEqual(sourceId, result.Item.ContentItemId);
                if (translatedId is not null)
                {
                    Assert.Equal(translatedId, result.Item.ContentItemId);
                }
                translatedId = result.Item.ContentItemId;
            });
        }
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var content = services.GetRequiredService<IContentManager>();
            var source = await content.GetAsync(sourceId, VersionOptions.Latest);
            var target = await content.GetAsync(translatedId, VersionOptions.Latest);
            Assert.True(source.Published);
            Assert.Equal("en", source.Get<LocalizationPart>(nameof(LocalizationPart)).Culture);
            Assert.Equal(source.Get<LocalizationPart>(nameof(LocalizationPart)).LocalizationSet, target.Get<LocalizationPart>(nameof(LocalizationPart)).LocalizationSet);
            Assert.Equal(source.Get<TitlePart>(nameof(TitlePart)).Title, target.Get<TitlePart>(nameof(TitlePart)).Title);
            Assert.Null(await content.GetAsync(translatedId, VersionOptions.Published));
            Assert.False((await CreateService(services).LocalizeAsync(s_user, sourceId, "en")).Created);
            var controller = ActivatorUtilities.CreateInstance<LocalizationAdminController>(services, CreateService(services));
            controller.ControllerContext = new() { HttpContext = Http(services) };
            controller.Url = Mock.Of<IUrlHelper>();
            var result = Assert.IsType<RedirectToActionResult>(await controller.Localize(sourceId, "de", "/return"));
            Assert.NotEqual(sourceId, result.RouteValues["contentItemId"]);
            Assert.Equal("/return", result.RouteValues["returnUrl"]);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var controller = ActivatorUtilities.CreateInstance<LocalizationAdminController>(services, CreateService(services));
            controller.ControllerContext = new() { HttpContext = Http(services) };
            controller.Url = Mock.Of<IUrlHelper>();
            Assert.Equal(sourceId, Assert.IsType<RedirectToActionResult>(await controller.Localize(sourceId, "de")).RouteValues["contentItemId"]);
        });
    }

    [Fact]
    public async Task List_VersionSelectionAndResourcePermissions_DeduplicatesAndFiltersVariants()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await PrepareAsync(context);
        string translatedId = null;
        await context.UsingTenantScopeAsync(async scope => translatedId = (await CreateService(scope.ServiceProvider).LocalizeAsync(s_user, id, "fr")).ContentItem.ContentItemId);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            Assert.Single(await ListAsync(services, id, "published"));
            Assert.Equal(2, (await ListAsync(services, id, "latest")).Length);
            Assert.Single(await ListAsync(services, id, "latest", Authorize((item, _) => item?.ContentItemId != translatedId)));
            var content = services.GetRequiredService<IContentManager>();
            await content.PublishAsync(await content.GetAsync(translatedId, VersionOptions.Latest));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var item = await scope.ServiceProvider.GetRequiredService<IContentManager>().GetAsync(translatedId, VersionOptions.DraftRequired);
            item.DisplayText = "private draft";
            await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().SaveAsync(item);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var latest = await ListAsync(services, id, "latest");
            Assert.Equal(2, latest.Length);
            Assert.Equal("private draft", latest.Single(item => item.ContentItemId == translatedId).DisplayText);
            var published = await ListAsync(services, id, "published");
            Assert.Equal(2, published.Length);
            Assert.DoesNotContain(published, item => item.DisplayText == "private draft");
            Assert.Single(await ListAsync(services, id, "latest", Authorize((_, requirement) => requirement.Permission.Name != CommonPermissions.PreviewContent.Name)));
        });
    }

    [Theory]
    [InlineData("LocalizeContent")]
    [InlineData("EditContent")]
    public async Task Localize_MissingSourceOrTypePermission_DoesNotCreateVariant(string denied)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await PrepareAsync(context);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var result = await CreateService(services, Authorize((_, requirement) => requirement.Permission.Name != denied)).LocalizeAsync(s_user, id, "fr");
            Assert.Equal(ContentLocalizationStatus.Forbidden, result.Status);
            Assert.Null(result.ContentItem);
            Assert.Single(await ListAsync(services, id, "latest"));
        });
    }

    [Fact]
    public async Task Localize_InvalidInputAndMissingPart_DoesNotChangeContent()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await PrepareAsync(context);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var service = CreateService(services);
            Assert.Equal(ContentLocalizationStatus.Invalid, (await service.LocalizeAsync(s_user, id, "unsupported")).Status);
            Assert.Equal(ContentLocalizationStatus.NotFound, (await service.LocalizeAsync(s_user, "missing", "fr")).Status);
            Assert.Single(await ListAsync(services, id, "latest"));
            Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await ContentLocalizationEndpoints.LocalizeAsync(Http(services), Authorize(), service, id, new())).StatusCode);
            var manager = services.GetRequiredService<IContentManager>();
            var page = await manager.NewAsync("Page");
            await manager.CreateAsync(page, VersionOptions.Draft);
            Assert.Equal(ContentLocalizationStatus.NotFound, (await service.LocalizeAsync(s_user, page.ContentItemId, "fr")).Status);
        });
    }

    [Fact]
    public async Task Localize_ExistingTargetNotEditable_RefusesRetryWithoutDisclosingIt()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await PrepareAsync(context);
        string targetId = null;
        await context.UsingTenantScopeAsync(async scope => targetId = (await CreateService(scope.ServiceProvider).LocalizeAsync(s_user, id, "fr")).ContentItem.ContentItemId);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var auth = Authorize((item, _) => item?.ContentItemId != targetId);
            var result = await CreateService(services, auth).LocalizeAsync(s_user, id, "fr");
            Assert.Equal(ContentLocalizationStatus.Forbidden, result.Status);
            Assert.Null(result.ContentItem);
            var controller = ActivatorUtilities.CreateInstance<LocalizationAdminController>(services, CreateService(services, auth));
            controller.ControllerContext = new() { HttpContext = Http(services) };
            controller.Url = Mock.Of<IUrlHelper>();
            Assert.IsType<ForbidResult>(await controller.Localize(id, "fr"));
        });
    }

    [Fact]
    public async Task Localize_PublishedVariantHasMovedDraft_ReportsConflict()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await PrepareAsync(context);
        string targetId = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var result = await CreateService(services).LocalizeAsync(s_user, id, "fr");
            targetId = result.ContentItem.ContentItemId;
            await services.GetRequiredService<IContentManager>().PublishAsync(result.ContentItem);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var target = await scope.ServiceProvider.GetRequiredService<IContentManager>().GetAsync(targetId, VersionOptions.DraftRequired);
            target.Alter<LocalizationPart>(part => part.Culture = "de");
            await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().SaveAsync(target);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var result = await CreateService(scope.ServiceProvider).LocalizeAsync(s_user, id, "fr");
            Assert.Equal(ContentLocalizationStatus.Conflict, result.Status);
            Assert.Null(result.ContentItem);
        });
    }

    [Fact]
    public async Task Endpoints_RemotePermissionAndFeatureLifecycle_AreEnforced()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await PrepareAsync(context);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var denied = Authorize((_, requirement) => requirement.Permission.Name != RemoteManagementPermissions.AccessRemoteManagement.Name);
            var result = await ContentLocalizationEndpoints.LocalizeAsync(Http(services), denied, CreateService(services), id, new() { Culture = "fr" });
            Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
            Assert.NotNull(services.GetService<IContentLocalizationService>());
        });
        await SetFeatureAsync(context, false);
        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.Null(scope.ServiceProvider.GetService<IContentLocalizationService>());
            return Task.CompletedTask;
        });
    }

    private static async Task<string> PrepareAsync(SiteContext context)
    {
        await SetFeatureAsync(context, true);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var siteService = services.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Alter<LocalizationSettings>(settings => { settings.DefaultCulture = "en"; settings.SupportedCultures = ["en", "fr", "de"]; });
            await siteService.UpdateSiteSettingsAsync(site);
            await services.GetRequiredService<IContentDefinitionManager>().AlterTypeDefinitionAsync("LocalizedProbe", type => type.Versionable().Draftable().WithPart("TitlePart").WithPart("LocalizationPart"));
        });
        string id = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var content = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var item = await content.NewAsync("LocalizedProbe");
            item.DisplayText = "Source";
            item.Alter<TitlePart>(part => part.Title = "Source title");
            await content.CreateAsync(item, VersionOptions.Published);
            id = item.ContentItemId;
        });
        return id;
    }

    private static Task SetFeatureAsync(SiteContext context, bool enabled) => context.UsingTenantScopeAsync(async scope =>
    {
        var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
        var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.ContentLocalization");
        if (enabled)
        {
            await manager.EnableFeaturesAsync([feature], force: true);
        }
        else
        {
            await manager.DisableFeaturesAsync([feature], force: true);
        }
    });

    private static DefaultHttpContext Http(IServiceProvider services) => new() { User = s_user, RequestServices = services };

    private static ContentLocalizationService CreateService(IServiceProvider services, IAuthorizationService authorization = null) =>
        ActivatorUtilities.CreateInstance<ContentLocalizationService>(services, authorization ?? Authorize());

    private static async Task<ContentLocalizationResponse[]> ListAsync(IServiceProvider services, string id, string version, IAuthorizationService authorization = null) =>
        Assert.IsType<Ok<ContentLocalizationResponse[]>>(await ContentLocalizationEndpoints.ListAsync(Http(services), authorization ?? Authorize(),
            services.GetRequiredService<IContentManager>(), services.GetRequiredService<IContentLocalizationManager>(), id, version)).Value;

    private static IAuthorizationService Authorize(Func<ContentItem, PermissionRequirement, bool> predicate = null)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object resource, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => predicate?.Invoke(resource as ContentItem, requirement) != false)
                    ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
