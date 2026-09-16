using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.HomeRoute.Endpoints;
using OrchardCore.Security;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Title.Models;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class HomeRouteIntegrationTests
{
    [Fact]
    public async Task SharedHandlerAndRemoteSelection_PersistAndPreserveUnpublishedDraft()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        string id = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var item = await manager.NewAsync("BlogPost");
            item.Alter<TitlePart>(part => part.Title = "Homepage published");
            item.Alter<AutoroutePart>(part => part.Path = "homepage-test");
            await manager.CreateAsync(item, VersionOptions.Published);
            id = item.ContentItemId;
            var part = item.Get<AutoroutePart>(nameof(AutoroutePart));
            part.SetHomepage = true;
            var handler = scope.ServiceProvider.GetRequiredService<AutoroutePartHandler>();
            await handler.PublishedAsync(new PublishContentContext(item, null), part);
            Assert.False(part.SetHomepage);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var draft = await manager.GetAsync(id, VersionOptions.DraftRequired);
            draft.Alter<TitlePart>(part => part.Title = "Keep this draft");
            await manager.UpdateAsync(draft);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IContentManager>();
            var result = await HomeRouteManagementEndpoints.SetHomeContentAsync(new DefaultHttpContext { RequestServices = services },
                id, Authorize(), manager, services.GetRequiredService<IHomeRouteService>());
            Assert.Equal(200, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
            Assert.Equal("Homepage published", (await manager.GetAsync(id, VersionOptions.Published)).Get<TitlePart>(nameof(TitlePart)).Title);
            Assert.Equal("Keep this draft", (await manager.GetAsync(id, VersionOptions.Latest)).Get<TitlePart>(nameof(TitlePart)).Title);
            Assert.False((await manager.GetAsync(id, VersionOptions.Latest)).Published);
        });
        var response = await context.Client.GetAsync("", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        Assert.Contains("Homepage published", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    private static IAuthorizationService Authorize(Func<ContentItem, PermissionRequirement, bool> allowed = null)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object resource, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => allowed?.Invoke(resource as ContentItem, requirement) ?? true)
                    ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
