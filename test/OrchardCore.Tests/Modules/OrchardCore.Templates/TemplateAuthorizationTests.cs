using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Autoroute;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentPreview;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Templates;
using OrchardCore.Templates.Controllers;
using OrchardCore.Templates.Settings;
using LiquidPermissions = OrchardCore.Liquid.Permissions;
using TemplateAdminMenu = OrchardCore.Templates.AdminMenu;
using TemplatePermissions = OrchardCore.Templates.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Templates;

public class TemplateAuthorizationTests
{
    [Fact]
    public async Task AdminNavigation_RequiresLiquidPermission()
    {
        var templateMenuBuilder = new NavigationBuilder();
        await new TemplateAdminMenu(CreateLocalizer<TemplateAdminMenu>())
            .BuildNavigationAsync(NavigationConstants.AdminId, templateMenuBuilder);

        var templatesItem = Assert.Single(Assert.Single(templateMenuBuilder.Build()).Items);
        Assert.Collection(
            templatesItem.Permissions,
            permission => Assert.Same(TemplatePermissions.ManageTemplates, permission),
            permission => Assert.Same(LiquidPermissions.ManageLiquidTemplates, permission));

        var adminTemplateMenuBuilder = new NavigationBuilder();
        await new AdminTemplatesAdminMenu(CreateLocalizer<AdminTemplatesAdminMenu>())
            .BuildNavigationAsync(NavigationConstants.AdminId, adminTemplateMenuBuilder);

        var adminTemplatesItem = Assert.Single(Assert.Single(adminTemplateMenuBuilder.Build()).Items);
        Assert.Collection(
            adminTemplatesItem.Permissions,
            permission => Assert.Same(AdminTemplatesPermissions.ManageAdminTemplates, permission),
            permission => Assert.Same(LiquidPermissions.ManageLiquidTemplates, permission));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public async Task PreviewIndex_RequiresBothPermissions(
        bool manageTemplates,
        bool manageLiquidTemplates,
        bool authorized)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity("Test")),
        };
        var controller = new PreviewController(
            Mock.Of<IContentManager>(),
            Mock.Of<IContentHandleManager>(),
            Mock.Of<IContentItemDisplayManager>(),
            CreateAuthorizationService(manageTemplates, manageLiquidTemplates).Object,
            Mock.Of<ISiteService>(),
            Mock.Of<IUpdateModelAccessor>(),
            new HttpContextAccessor { HttpContext = httpContext })
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            },
        };

        var result = await controller.Index();

        if (authorized)
        {
            Assert.IsType<ViewResult>(result);
        }
        else
        {
            Assert.IsType<ForbidResult>(result);
        }
    }

    [Fact]
    public async Task TemplateShortcuts_RequireBothPermissions()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity("Test")),
            },
        };
        var authorizationService = CreateAuthorizationService(
            manageTemplates: true,
            manageLiquidTemplates: false);

        var partDriver = new TemplateContentPartDefinitionDriver(
            CreateLocalizer<TemplateContentPartDefinitionDriver>(),
            authorizationService.Object,
            httpContextAccessor);
        var typeDriver = new TemplateContentTypeDefinitionDriver(
            CreateLocalizer<TemplateContentTypeDefinitionDriver>(),
            authorizationService.Object,
            httpContextAccessor);
        var typePartDriver = new TemplateContentTypePartDefinitionDriver(
            CreateLocalizer<TemplateContentTypePartDefinitionDriver>(),
            authorizationService.Object,
            httpContextAccessor);

        Assert.Null(await partDriver.EditAsync(
            new ContentPartDefinition("Part"),
            null));
        Assert.Null(await typeDriver.EditAsync(
            new ContentTypeDefinition("Type", "Type"),
            null));
        Assert.Null(await typePartDriver.EditAsync(
            new ContentTypePartDefinition(
                "Part",
                new ContentPartDefinition("Part"),
                new JsonObject()),
            null));

        authorizationService = CreateAuthorizationService(
            manageTemplates: true,
            manageLiquidTemplates: true);

        partDriver = new TemplateContentPartDefinitionDriver(
            CreateLocalizer<TemplateContentPartDefinitionDriver>(),
            authorizationService.Object,
            httpContextAccessor);
        typeDriver = new TemplateContentTypeDefinitionDriver(
            CreateLocalizer<TemplateContentTypeDefinitionDriver>(),
            authorizationService.Object,
            httpContextAccessor);
        typePartDriver = new TemplateContentTypePartDefinitionDriver(
            CreateLocalizer<TemplateContentTypePartDefinitionDriver>(),
            authorizationService.Object,
            httpContextAccessor);

        Assert.NotNull(await partDriver.EditAsync(
            new ContentPartDefinition("Part"),
            null));
        Assert.NotNull(await typeDriver.EditAsync(
            new ContentTypeDefinition("Type", "Type"),
            null));
        Assert.NotNull(await typePartDriver.EditAsync(
            new ContentTypePartDefinition(
                "Part",
                new ContentPartDefinition("Part"),
                new JsonObject()),
            null));
    }

    private static Mock<IAuthorizationService> CreateAuthorizationService(
        bool manageTemplates,
        bool manageLiquidTemplates)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                null,
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((
                ClaimsPrincipal _,
                object _,
                IEnumerable<IAuthorizationRequirement> requirements) =>
            {
                var permission = Assert.Single(requirements.OfType<PermissionRequirement>()).Permission;
                var isGranted = permission.Name switch
                {
                    nameof(TemplatePermissions.ManageTemplates) => manageTemplates,
                    nameof(LiquidPermissions.ManageLiquidTemplates) => manageLiquidTemplates,
                    _ => false,
                };

                return isGranted ? AuthorizationResult.Success() : AuthorizationResult.Failed();
            });

        return authorizationService;
    }

    private static IStringLocalizer<T> CreateLocalizer<T>()
    {
        var localizer = new Mock<IStringLocalizer<T>>();
        localizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        localizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) =>
                new LocalizedString(name, string.Format(name, arguments)));

        return localizer.Object;
    }
}
