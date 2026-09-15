using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Liquid;
using OrchardCore.Liquid.Fields;
using OrchardCore.Liquid.Handlers;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Liquid;

public class LiquidSecurityTests
{
    [Fact]
    public async Task ManageLiquidTemplates_IsSecurityCriticalAndGrantedToExpectedRoles()
    {
        var provider = new Permissions();

        var permission = Assert.Single(await provider.GetPermissionsAsync());
        Assert.Same(Permissions.ManageLiquidTemplates, permission);
        Assert.True(permission.IsSecurityCritical);

        var stereotypes = provider.GetDefaultStereotypes().ToArray();
        Assert.Collection(
            stereotypes,
            stereotype =>
            {
                Assert.Equal(OrchardCoreConstants.Roles.Administrator, stereotype.Name);
                Assert.Contains(permission, stereotype.Permissions);
            },
            stereotype =>
            {
                Assert.Equal(OrchardCoreConstants.Roles.Editor, stereotype.Name);
                Assert.Contains(permission, stereotype.Permissions);
            });
    }

    [Fact]
    public async Task LiquidFieldValidation_WithoutPermission_Fails()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                null,
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        var localizer = new Mock<IStringLocalizer<LiquidFieldHandler>>();
        localizer
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity()),
        };
        var handler = new LiquidFieldHandler(
            new HttpContextAccessor { HttpContext = httpContext },
            authorizationService.Object,
            Mock.Of<ILiquidTemplateManager>(),
            localizer.Object);
        var context = new ValidateContentFieldContext(new ContentItem())
        {
            PartName = "Article",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(
                new ContentFieldDefinition(nameof(LiquidField)),
                "Template",
                new JsonObject()),
        };

        await handler.ValidatingAsync(context, new LiquidField());

        Assert.False(context.ContentValidateResult.Succeeded);
        Assert.Contains(
            context.ContentValidateResult.Errors,
            error => error.ErrorMessage.Contains("permission", StringComparison.OrdinalIgnoreCase));
    }
}
