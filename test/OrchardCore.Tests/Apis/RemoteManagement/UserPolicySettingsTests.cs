using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Users.Drivers;
using OrchardCore.Liquid;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services.Management;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class UserPolicySettingsTests
{
    [Theory]
    [InlineData("{\"disableLocalLogin\":true,\"password\":\"ignored-secret\"}")]
    [InlineData("{\"disableLocalLogin\":true,\"allowRememberMe\":null}")]
    [InlineData("{\"disableLocalLogin\":\"true\"}")]
    public async Task InvalidLoginPatchDoesNotPartiallyMutate(string json)
    {
        var fixture = Create(new LoginSettingsManagementStartup());
        using var services = fixture.Services;
        var provider = services.GetRequiredService<ISiteSettingsSectionProvider>();
        var before = (await provider.GetAsync()).Values.ToJsonString();
        var result = await provider.UpdateAsync(JsonNode.Parse(json).AsObject());
        Assert.NotEmpty(result.Errors);
        Assert.Equal(before, (await provider.GetAsync()).Values.ToJsonString());
        fixture.SiteService.Verify(value => value.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task LoginPolicyPreservesOmittedValuesAndSkipsEquivalentRetries()
    {
        var fixture = Create(new LoginSettingsManagementStartup());
        using var services = fixture.Services;
        var provider = services.GetRequiredService<ISiteSettingsSectionProvider>();
        var patch = new JsonObject { ["allowRememberMe"] = false };
        var result = await provider.UpdateAsync(patch);
        Assert.Empty(result.Errors);
        Assert.True(result.Changed);
        Assert.True(result.Section.Values["allowChangingPhoneNumber"].GetValue<bool>());
        Assert.False((await provider.UpdateAsync(patch)).Changed);
        Assert.False((await provider.UpdateAsync([])).Changed);
        fixture.SiteService.Verify(value => value.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task InvalidRecoveryCountPreservesMfaPolicy(int count)
    {
        var fixture = Create(new TwoFactorLoginSettingsManagementStartup());
        using var services = fixture.Services;
        var provider = services.GetRequiredService<ISiteSettingsSectionProvider>();
        var result = await provider.UpdateAsync(new JsonObject
        {
            ["requireTwoFactorAuthentication"] = true, ["numberOfRecoveryCodesToGenerate"] = count,
        });
        Assert.NotEmpty(result.Errors);
        Assert.False((await provider.GetAsync()).Values["requireTwoFactorAuthentication"].GetValue<bool>());
    }

    [Theory]
    [InlineData(5)]
    [InlineData(8)]
    public async Task AuthenticatorRejectsUnsupportedCodeLengths(int length)
    {
        var fixture = Create(new AuthenticatorAppLoginSettingsManagementStartup());
        using var services = fixture.Services;
        var provider = services.GetRequiredService<ISiteSettingsSectionProvider>();
        var result = await provider.UpdateAsync(new JsonObject { ["tokenLength"] = length });
        Assert.NotEmpty(result.Errors);
        Assert.Equal(6, (await provider.GetAsync()).Values["tokenLength"].GetValue<int>());
    }

    [Fact]
    public void SharedApplyPreservesInvalidMfaValues()
    {
        var current = new TwoFactorLoginSettings();
        Assert.False(UserPolicySettingsEditor.Apply(current, new TwoFactorLoginSettings
        {
            RequireTwoFactorAuthentication = true, NumberOfRecoveryCodesToGenerate = 0,
        }));
        Assert.False(current.RequireTwoFactorAuthentication);
        Assert.Equal(5, current.NumberOfRecoveryCodesToGenerate);
    }

    [Fact]
    public async Task ExistingMfaEditorDoesNotMutateOnInvalidRecoveryCount()
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<TwoFactorLoginSettings>(), It.IsAny<string>()))
            .Callback((TwoFactorLoginSettings model, string _) =>
            {
                model.RequireTwoFactorAuthentication = true;
                model.NumberOfRecoveryCodesToGenerate = 0;
            }).ReturnsAsync(true);
        var driver = new TwoFactorLoginSettingsDisplayDriver(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, authorization.Object,
            new StringLocalizer<TwoFactorLoginSettingsDisplayDriver>(new global::OrchardCore.Localization.NullStringLocalizerFactory()));
        var context = new UpdateEditorContext(new Shape(), LoginSettingsDisplayDriver.GroupId, false, "",
            Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object);
        var settings = new TwoFactorLoginSettings();
        await driver.UpdateAsync(new SiteSettings(), settings, context);
        Assert.False(updater.Object.ModelState.IsValid);
        Assert.False(settings.RequireTwoFactorAuthentication);
        Assert.Equal(5, settings.NumberOfRecoveryCodesToGenerate);
    }

    [Fact]
    public async Task DisabledRolePolicyCanRetainADeletedRole()
    {
        var settings = new RoleLoginSettings { Roles = ["DeletedRole"], RequireTwoFactorAuthenticationForSpecificRoles = false };
        var roles = new Mock<global::OrchardCore.Security.Services.IRoleService>(MockBehavior.Strict);
        Assert.Empty(await UserPolicySettingsEditor.ValidateAsync(settings, roles.Object));
        Assert.Equal(["DeletedRole"], settings.Roles);
    }

    [Fact]
    public async Task ExistingEmailTemplateEditorPreservesInvalidInput()
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<EmailAuthenticatorLoginSettings>(), It.IsAny<string>()))
            .Callback((EmailAuthenticatorLoginSettings model, string _) => { model.Body = "{% if %}"; model.Subject = "changed"; })
            .ReturnsAsync(true);
        IEnumerable<string> errors = ["Invalid template"];
        var liquid = new Mock<ILiquidTemplateManager>();
        liquid.Setup(value => value.Validate(It.IsAny<string>(), out errors)).Returns(false);
        var driver = new EmailAuthenticatorLoginSettingsDisplayDriver(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, authorization.Object, liquid.Object);
        var context = new UpdateEditorContext(new Shape(), LoginSettingsDisplayDriver.GroupId, false, "",
            Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object);
        var settings = new EmailAuthenticatorLoginSettings { Subject = "original", Body = "valid" };
        await driver.UpdateAsync(new SiteSettings(), settings, context);
        Assert.False(updater.Object.ModelState.IsValid);
        Assert.Equal("original", settings.Subject);
        Assert.Equal("valid", settings.Body);
    }

    private static (ServiceProvider Services, Mock<ISiteService> SiteService) Create(global::OrchardCore.Modules.StartupBase startup)
    {
        var site = new SiteSettings { IsReadOnly = false };
        var service = new Mock<ISiteService>();
        service.Setup(value => value.GetSiteSettingsAsync()).ReturnsAsync(site);
        service.Setup(value => value.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var services = new ServiceCollection().AddSingleton(service.Object);
        startup.ConfigureServices(services);
        return (services.BuildServiceProvider(), service);
    }
}
