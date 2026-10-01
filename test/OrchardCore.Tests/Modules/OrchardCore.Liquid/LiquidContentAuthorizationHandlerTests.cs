using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Liquid.Security;
using OrchardCore.Security;
using LiquidPermissions = OrchardCore.Liquid.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Liquid;

public class LiquidContentAuthorizationHandlerTests
{
    [Theory]
    [InlineData("LiquidPart")]
    [InlineData("FacebookPluginPart")]
    public async Task EditingTypesWithLiquidTemplatePartsShouldRequireLiquidPermission(string partName)
    {
        var definition = CreateTypeDefinition(new ContentTypePartDefinition(partName, new ContentPartDefinition(partName), []));

        Assert.False(await AuthorizeEditAsync(definition, canManageLiquidTemplates: false));
        Assert.True(await AuthorizeEditAsync(definition, canManageLiquidTemplates: true));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData("true", false)]
    [InlineData("True", false)]
    [InlineData(false, true)]
    [InlineData("false", true)]
    [InlineData("invalid", true)]
    public async Task RenderLiquidSettingShouldBeReadFromBooleanOrStringValues(object renderLiquid, bool expected)
    {
        var settings = new JsonObject
        {
            ["HtmlBodyPartSettings"] = new JsonObject
            {
                ["RenderLiquid"] = renderLiquid is bool value ? JsonValue.Create(value) : JsonValue.Create((string)renderLiquid),
            },
        };
        var definition = CreateTypeDefinition(new ContentTypePartDefinition("HtmlBodyPart", new ContentPartDefinition("HtmlBodyPart"), settings));

        Assert.Equal(expected, await AuthorizeEditAsync(definition, canManageLiquidTemplates: false));
    }

    private static ContentTypeDefinition CreateTypeDefinition(ContentTypePartDefinition partDefinition) =>
        new("Article", "Article", [partDefinition], []);

    private static async Task<bool> AuthorizeEditAsync(ContentTypeDefinition definition, bool canManageLiquidTemplates)
    {
        var contentDefinitionManager = new Mock<IContentDefinitionManager>();
        contentDefinitionManager
            .Setup(manager => manager.GetTypeDefinitionAsync(definition.Name))
            .ReturnsAsync(definition);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.Is<IEnumerable<IAuthorizationRequirement>>(requirements => requirements
                    .OfType<PermissionRequirement>()
                    .Any(requirement => requirement.Permission.Name == LiquidPermissions.ManageLiquidTemplates.Name))))
            .ReturnsAsync(canManageLiquidTemplates ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        var services = new ServiceCollection()
            .AddSingleton(contentDefinitionManager.Object)
            .AddSingleton(authorizationService.Object)
            .BuildServiceProvider();

        var requirement = new PermissionRequirement(CommonPermissions.EditContent);
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity("Test")),
            new ContentItem { ContentType = definition.Name });

        await new LiquidContentAuthorizationHandler(services).HandleAsync(context);

        return !context.HasFailed;
    }
}
