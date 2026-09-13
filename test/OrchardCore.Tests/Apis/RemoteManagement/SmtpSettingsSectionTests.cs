using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Email.Smtp.ViewModels;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class SmtpSettingsSectionTests
{
    [Theory]
    [InlineData("{\"host\":\"changed\",\"port\":65536}")]
    [InlineData("{\"host\":\"changed\",\"encryptionMethod\":\"1\"}")]
    [InlineData("{\"host\":\"changed\",\"password\":null}")]
    [InlineData("{\"host\":\"changed\",\"hasPassword\":true}")]
    [InlineData("{\"defaultSender\":\"invalid\"}")]
    [InlineData("{\"deliveryMethod\":\"SpecifiedPickupDirectory\",\"pickupDirectoryLocation\":\"../outside\"}")]
    [InlineData("{\"password\":\"replacement\",\"clearPassword\":true}")]
    public async Task InvalidPatchPreservesSettingsAndDoesNotSignal(string json)
    {
        var fixture = Create();
        var before = fixture.Site.Properties.ToJsonString();
        var result = await fixture.Provider.UpdateAsync(JsonNode.Parse(json).AsObject());
        Assert.NotEmpty(result.Errors);
        Assert.Equal(before, fixture.Site.Properties.ToJsonString());
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Notifier.Verify(notifier => notifier.RequestUpdate<SmtpOptions>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PasswordIsProtectedRedactedRetainedOnOmissionAndExplicitlyCleared()
    {
        var fixture = Create();
        var patch = new JsonObject { ["password"] = "test-secret" };
        var result = await fixture.Provider.UpdateAsync(patch);
        Assert.Empty(result.Errors);
        Assert.True(result.Changed);
        var stored = fixture.Site.GetOrCreate<SmtpSettings>().Password;
        Assert.NotEqual("test-secret", stored);
        Assert.Equal("test-secret", fixture.Protection.CreateProtector(SmtpOptionsConfiguration.ProtectorName).Unprotect(stored));
        Assert.False(result.Section.Values.ContainsKey("password"));
        Assert.True(result.Section.Values["hasPassword"].GetValue<bool>());
        Assert.DoesNotContain(stored, result.Section.Values.ToJsonString());
        Assert.False((await fixture.Provider.UpdateAsync(patch)).Changed);
        Assert.Equal(stored, fixture.Site.GetOrCreate<SmtpSettings>().Password);
        Assert.False((await fixture.Provider.UpdateAsync([])).Changed);
        var cleared = await fixture.Provider.UpdateAsync(new JsonObject { ["clearPassword"] = true });
        Assert.True(cleared.Changed);
        Assert.False(cleared.Section.Values["hasPassword"].GetValue<bool>());
        Assert.Null(fixture.Site.GetOrCreate<SmtpSettings>().Password);
        Assert.False((await fixture.Provider.UpdateAsync(new JsonObject { ["clearPassword"] = true })).Changed);
    }

    [Fact]
    public async Task DisableClearsDefaultProviderAndRetainsConfiguration()
    {
        var fixture = Create();
        var result = await fixture.Provider.UpdateAsync(new JsonObject { ["isEnabled"] = false });
        Assert.True(result.Changed);
        Assert.Null(fixture.Site.GetOrCreate<EmailSettings>().DefaultProviderName);
        Assert.Equal("localhost", fixture.Site.GetOrCreate<SmtpSettings>().Host);
        Assert.False((await fixture.Provider.UpdateAsync(new JsonObject { ["isEnabled"] = false })).Changed);
        var enabled = await fixture.Provider.UpdateAsync(new JsonObject { ["isEnabled"] = true });
        Assert.True(enabled.Changed);
        Assert.Equal(SmtpEmailProvider.TechnicalName, fixture.Site.GetOrCreate<EmailSettings>().DefaultProviderName);
    }

    [Fact]
    public async Task ExistingAdminEditorRejectsInvalidValuesBeforeMutatingSettings()
    {
        var fixture = Create();
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<SmtpSettingsViewModel>(), It.IsAny<string>()))
            .Callback((SmtpSettingsViewModel model, string _) =>
            {
                model.IsEnabled = true;
                model.DefaultSender = "invalid";
                model.Host = "changed-host";
                model.Password = "must-not-persist";
            }).ReturnsAsync(true);
        var driver = new SmtpSettingsDisplayDriver(fixture.Notifier.Object, fixture.Protection,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            Mock.Of<IOptionsMonitor<SmtpOptions>>(value => value.CurrentValue == new SmtpOptions()),
            authorization.Object, Mock.Of<IEmailAddressValidator>(),
            new StringLocalizer<SmtpSettingsDisplayDriver>(new global::OrchardCore.Localization.NullStringLocalizerFactory()));
        var context = new UpdateEditorContext(new Shape(), EmailSettings.GroupId, false, "",
            Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object);
        var settings = fixture.Site.GetOrCreate<SmtpSettings>();
        await driver.UpdateAsync(fixture.Site, settings, context);
        Assert.False(updater.Object.ModelState.IsValid);
        Assert.Equal("localhost", settings.Host);
        Assert.Null(settings.Password);
        fixture.Notifier.Verify(value => value.RequestUpdate<SmtpOptions>(It.IsAny<string>()), Times.Never);
    }

    private static Fixture Create() => new();

    private sealed class Fixture
    {
        public SiteSettings Site { get; } = new() { IsReadOnly = false };
        public Mock<ISiteService> Service { get; } = new();
        public Mock<IOptionsUpdateNotifier> Notifier { get; } = new();
        public EphemeralDataProtectionProvider Protection { get; } = new();
        public SmtpSettingsSectionProvider Provider { get; }

        public Fixture()
        {
            Site.Put(new SmtpSettings { IsEnabled = true, DefaultSender = "admin@example.com", Host = "localhost", PickupDirectoryLocation = "/" });
            Site.Put(new EmailSettings { DefaultProviderName = SmtpEmailProvider.TechnicalName });
            Service.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(Site);
            Service.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(Site);
            Notifier.Setup(notifier => notifier.RequestUpdate<SmtpOptions>(It.IsAny<string>())).Returns(Notifier.Object);
            Notifier.Setup(notifier => notifier.RequestUpdate<EmailProviderOptions>(It.IsAny<string>())).Returns(Notifier.Object);
            Notifier.Setup(notifier => notifier.RequestUpdate<EmailOptions>(It.IsAny<string>())).Returns(Notifier.Object);
            var options = Mock.Of<IOptionsMonitor<SmtpOptions>>(value => value.CurrentValue == new SmtpOptions());
            var validator = new Mock<IEmailAddressValidator>();
            validator.Setup(value => value.Validate(It.IsAny<string>())).Returns((string value) => value == "admin@example.com");
            Provider = new SmtpSettingsSectionProvider(Service.Object, options, Notifier.Object, Protection,
                validator.Object, new StringLocalizer<SmtpSettingsSectionProvider>(new global::OrchardCore.Localization.NullStringLocalizerFactory()));
        }
    }
}
