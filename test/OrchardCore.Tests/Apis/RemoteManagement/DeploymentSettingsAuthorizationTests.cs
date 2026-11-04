using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Deployment;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Deployment;
using OrchardCore.Recipes.Models;
using OrchardCore.Settings;
using OrchardCore.Users.Deployment;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentSettingsAuthorizationTests
{
    [Fact]
    public async Task UserExport_RequiresManageUsersBeforeReadingCredentialRecords()
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());
        var session = new Mock<YesSql.ISession>(MockBehavior.Strict);
        var source = new AllUsersDeploymentSource(session.Object, authorization.Object);
        var result = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor())
        {
            User = new ClaimsPrincipal(new ClaimsIdentity("Deployment")),
        };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => source.ProcessDeploymentStepAsync(new AllUsersDeploymentStep(), result));
        Assert.Empty(result.Steps);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CustomSettings_UsesExplicitInitiatorWithoutHttpAndFailsClosed(bool allowed)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "initiator")], "Deployment"));
        var type = new ContentTypeDefinition("Preferences", "Preferences", [],
            new JsonObject { ["ContentTypeSettings"] = new JsonObject { ["Stereotype"] = "CustomSettings" } });
        var definitions = new Mock<IContentDefinitionManager>();
        definitions.Setup(manager => manager.ListTypeDefinitionsAsync()).ReturnsAsync([type]);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(principal, It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        var siteMock = new Mock<ISite>();
        siteMock.SetupGet(site => site.Properties).Returns(new JsonObject());
        var site = siteMock.Object;
        var sites = new Mock<ISiteService>();
        sites.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        var content = new Mock<IContentManager>();
        content.Setup(manager => manager.NewAsync(type.Name)).ReturnsAsync(new ContentItem { ContentType = type.Name });
        var settings = new CustomSettingsService(sites.Object, content.Object, new HttpContextAccessor(), authorization.Object, definitions.Object);
        var source = new CustomSettingsDeploymentSource(settings);
        var result = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor()) { User = principal };
        var step = new CustomSettingsDeploymentStep { IncludeAll = true };
        if (allowed)
        {
            await source.ProcessDeploymentStepAsync(step, result);
            Assert.NotNull(Assert.Single(result.Steps)[type.Name]);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => source.ProcessDeploymentStepAsync(step, result));
            Assert.Empty(result.Steps);
            content.Verify(manager => manager.NewAsync(It.IsAny<string>()), Times.Never);
        }
        authorization.Verify(service => service.AuthorizeAsync(principal, It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()), Times.Once);
    }
}
