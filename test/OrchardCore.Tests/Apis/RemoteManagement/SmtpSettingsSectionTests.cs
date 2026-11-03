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
    public async Task ChangingHost_StoredPassword_MustBeProvidedAgain()
    {
        var fixture = Create();
        await fixture.Provider.UpdateAsync(new JsonObject { ["password"] = "test-secret" });
        var host = fixture.Site.GetOrCreate<SmtpSettings>().Host;

        var rejected = await fixture.Provider.UpdateAsync(new JsonObject { ["host"] = "other.example.test" });

        Assert.True(rejected.Errors.ContainsKey("password"));
        Assert.Equal(host, fixture.Site.GetOrCreate<SmtpSettings>().Host);
        Assert.Empty((await fixture.Provider.UpdateAsync(new JsonObject { ["host"] = "other.example.test", ["clearPassword"] = true })).Errors);
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
