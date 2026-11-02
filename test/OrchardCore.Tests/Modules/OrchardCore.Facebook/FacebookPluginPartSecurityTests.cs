using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Facebook.Widgets.Drivers;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Facebook.Widgets.ViewModels;
using OrchardCore.Liquid;
using OrchardCore.Security;
using LiquidPermissions = OrchardCore.Liquid.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Facebook;

public class FacebookPluginPartSecurityTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public async Task Update_RequiresPermissionAndValidTemplate_BeforeAssigningSource(bool authorized, bool validTemplate)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(service => service.AuthorizeAsync(
            user,
            null,
            It.Is<IEnumerable<IAuthorizationRequirement>>(requirements =>
                requirements.OfType<PermissionRequirement>().Any(requirement =>
                    requirement.Permission == LiquidPermissions.ManageLiquidTemplates))))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        var modelState = new ModelStateDictionary();
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(model => model.ModelState).Returns(modelState);
        updater.Setup(model => model.TryUpdateModelAsync(
            It.IsAny<FacebookPluginPartViewModel>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<FacebookPluginPartViewModel, object>>[]>()))
            .Callback<FacebookPluginPartViewModel, string, Expression<Func<FacebookPluginPartViewModel, object>>[]>(
                (model, prefix, expressions) => model.Liquid = "{{ updated }}")
            .ReturnsAsync(true);
        var liquidManager = new Mock<ILiquidTemplateManager>();
        IEnumerable<string> errors = ["Invalid template"];
        liquidManager.Setup(manager => manager.Validate("{{ updated }}", out errors))
            .Returns(validTemplate);
        var localizer = new Mock<IStringLocalizer<FacebookPluginPartDisplayDriver>>();
        localizer.Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        localizer.Setup(localizer => localizer[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        var driver = new FacebookPluginPartDisplayDriver(
            Mock.Of<IContentDefinitionManager>(),
            liquidManager.Object,
            authorizationService.Object,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = user } },
            localizer.Object);
        var part = new FacebookPluginPart { Liquid = "{{ original }}" };
        var editorContext = new UpdateEditorContext(null, null, false, string.Empty, null, null, updater.Object);
        var context = new UpdatePartEditorContext(null, editorContext);

        var result = await driver.UpdateAsync(part, context);

        Assert.NotNull(result);
        Assert.Equal(authorized && validTemplate ? "{{ updated }}" : "{{ original }}", part.Liquid);
        Assert.Equal(authorized && validTemplate, modelState.IsValid);
        if (!authorized)
        {
            Assert.Contains(modelState.Values.SelectMany(value => value.Errors),
                error => error.ErrorMessage.Contains("permission", StringComparison.OrdinalIgnoreCase));
            updater.Verify(model => model.TryUpdateModelAsync(
                It.IsAny<FacebookPluginPartViewModel>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<FacebookPluginPartViewModel, object>>[]>()), Times.Never);
            liquidManager.Verify(manager => manager.Validate(It.IsAny<string>(), out errors), Times.Never);
        }
    }
}
