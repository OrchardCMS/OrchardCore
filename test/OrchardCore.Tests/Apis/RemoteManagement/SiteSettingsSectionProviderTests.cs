using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;
using OrchardCore.Google.Analytics.Services;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.Google.Authentication.Services;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.TagManager.Services;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.ReverseProxy.Services;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.Signin.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class SiteSettingsSectionProviderTests
{
    public static TheoryData<string> Sections =>
    [
        "recaptcha",
        "reverse-proxy",
        "google-analytics",
        "google-tag-manager",
        "google-authentication",
        "github-authentication",
        "microsoft-account",
        "microsoft-entra-id",
        "twitter",
        "twitter-signin",
        "facebook",
        "facebook-login",
        "facebook-pixel",
        "sms",
        "sms-twilio",
        "admin",
    ];

    [Theory]
    [MemberData(nameof(Sections))]
    public void Descriptor_Provider_NameMatchesAndSchemaIsClosed(string name)
    {
        var provider = new Fixture().Create(name);

        Assert.Equal(name, provider.Descriptor.Name);
        Assert.NotNull(provider.Descriptor.FeatureId);
        Assert.Same(provider.ReadPermission, provider.UpdatePermission);
        Assert.False(provider.GetSchema()["additionalProperties"].GetValue<bool>());
        Assert.NotEmpty(provider.GetSchema()["properties"].AsObject());
    }

    [Theory]
    [MemberData(nameof(Sections))]
    public async Task Update_UnknownProperty_IsRejectedAndNothingIsSaved(string name)
    {
        var fixture = new Fixture();
        var provider = fixture.Create(name);
        var before = fixture.Site.Properties.ToJsonString();

        var result = await provider.UpdateAsync(new JsonObject { ["notManaged"] = "value" });

        Assert.True(result.Errors.ContainsKey("notManaged"));
        Assert.Equal(before, fixture.Site.Properties.ToJsonString());
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Theory]
    [InlineData("recaptcha", "secretKey")]
    [InlineData("google-authentication", "clientSecret")]
    [InlineData("github-authentication", "clientSecret")]
    [InlineData("microsoft-account", "appSecret")]
    [InlineData("twitter", "consumerSecret")]
    [InlineData("twitter", "accessToken")]
    [InlineData("twitter", "accessTokenSecret")]
    [InlineData("facebook", "appSecret")]
    [InlineData("facebook-pixel", "conversionsApiAccessToken")]
    [InlineData("sms-twilio", "authToken")]
    public async Task Update_Secret_IsWriteOnlyAndNeverReadBack(string name, string property)
    {
        var fixture = new Fixture();
        var provider = fixture.Create(name);

        Assert.True(provider.GetSchema()["properties"][property]["writeOnly"].GetValue<bool>());
        Assert.True(provider.GetSchema().DeclaresWriteOnlyValue());
        Assert.True(provider.Descriptor.RequiresHttps);

        var result = await provider.UpdateAsync(Fixture.ValidValues(name, new JsonObject { [property] = "test-secret-value" }));

        Assert.Empty(result.Errors);
        Assert.True(result.Changed);
        Assert.Contains(property, result.Section.RedactedProperties);
        Assert.False(result.Section.Values.ContainsKey(property));
        Assert.True(result.Section.Values["has" + char.ToUpperInvariant(property[0]) + property[1..]].GetValue<bool>());

        var read = (await provider.GetAsync()).Values.ToJsonString();
        Assert.DoesNotContain("test-secret-value", read);
        Assert.DoesNotContain("test-secret-value", result.Section.Values.ToJsonString());
    }

    [Theory]
    [InlineData("reverse-proxy")]
    [InlineData("google-analytics")]
    [InlineData("google-tag-manager")]
    [InlineData("microsoft-entra-id")]
    [InlineData("twitter-signin")]
    [InlineData("facebook-login")]
    [InlineData("sms")]
    [InlineData("admin")]
    public void Schema_SectionWithoutSecret_DeclaresNoWriteOnlyValue(string name)
        => Assert.False(new Fixture().Create(name).GetSchema().DeclaresWriteOnlyValue());

    [Theory]
    [InlineData("google-analytics", "trackingId")]
    [InlineData("google-tag-manager", "containerId")]
    [InlineData("google-authentication", "clientId")]
    [InlineData("github-authentication", "clientId")]
    [InlineData("microsoft-account", "appId")]
    [InlineData("microsoft-entra-id", "displayName")]
    [InlineData("microsoft-entra-id", "appId")]
    [InlineData("microsoft-entra-id", "tenantId")]
    [InlineData("twitter", "consumerKey")]
    [InlineData("facebook", "appId")]
    public async Task Update_RequiredPropertyMissing_IsRejected(string name, string property)
    {
        var fixture = new Fixture();
        var provider = fixture.Create(name);
        var values = Fixture.ValidValues(name, []);
        values[property] = "   ";

        var result = await provider.UpdateAsync(values);

        Assert.True(result.Errors.ContainsKey(property));
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Theory]
    [InlineData("google-authentication")]
    [InlineData("github-authentication")]
    [InlineData("microsoft-account")]
    [InlineData("microsoft-entra-id")]
    [InlineData("twitter-signin")]
    [InlineData("facebook-login")]
    public async Task Update_CallbackPath_FollowsTheEditorFormat(string name)
    {
        var fixture = new Fixture();
        var provider = fixture.Create(name);

        var invalid = await provider.UpdateAsync(Fixture.ValidValues(name, new JsonObject { ["callbackPath"] = "no-leading-slash" }));
        Assert.True(invalid.Errors.ContainsKey("callbackPath"));

        var valid = await provider.UpdateAsync(Fixture.ValidValues(name, new JsonObject { ["callbackPath"] = "/signin-test" }));
        Assert.Empty(valid.Errors);
        Assert.Equal("/signin-test", valid.Section.Values["callbackPath"].GetValue<string>());
        Assert.True(valid.ReloadRequested);
        fixture.Release.Verify(release => release.RequestRelease(), Times.Once);

        var reset = await provider.UpdateAsync(new JsonObject { ["callbackPath"] = null });
        Assert.Empty(reset.Errors);
        Assert.Null(reset.Section.Values["callbackPath"]);
    }

    [Fact]
    public async Task Update_SameProtectedSecret_DoesNotChangeTheSettings()
    {
        var fixture = new Fixture();
        var provider = fixture.Create("google-authentication");
        var values = () => new JsonObject { ["clientId"] = "id", ["clientSecret"] = "test-secret-value" };

        Assert.True((await provider.UpdateAsync(values())).Changed);
        var stored = fixture.Site.GetOrCreate<GoogleAuthenticationSettings>().ClientSecret;

        var retry = await provider.UpdateAsync(values());

        Assert.False(retry.Changed);
        Assert.False(retry.ReloadRequested);
        Assert.Equal(stored, fixture.Site.GetOrCreate<GoogleAuthenticationSettings>().ClientSecret);
        Assert.NotEqual("test-secret-value", stored);
    }

    [Fact]
    public async Task Update_NewSecretValue_ReplacesTheSecretReference()
    {
        var fixture = new Fixture();
        fixture.Site.Put(new GitHubAuthenticationSettings { ClientID = "id", ClientSecretSecretName = "Stored.Secret" });
        var provider = fixture.Create("github-authentication");

        var section = await provider.GetAsync();
        Assert.True(section.Values["hasClientSecret"].GetValue<bool>());
        Assert.Equal("Stored.Secret", section.Values["clientSecretSecretName"].GetValue<string>());

        var result = await provider.UpdateAsync(new JsonObject { ["clientSecret"] = "test-secret-value" });

        Assert.Empty(result.Errors);
        var stored = fixture.Site.GetOrCreate<GitHubAuthenticationSettings>();
        Assert.Null(stored.ClientSecretSecretName);
        Assert.NotNull(stored.ClientSecret);
    }

    [Fact]
    public async Task Update_ClearSecret_RemovesValueAndReference()
    {
        var fixture = new Fixture();
        fixture.Site.Put(new GitHubAuthenticationSettings { ClientID = "id", ClientSecret = "protected", ClientSecretSecretName = "Stored.Secret" });
        var provider = fixture.Create("github-authentication");

        var result = await provider.UpdateAsync(new JsonObject { ["clearClientSecret"] = true });

        Assert.Empty(result.Errors);
        Assert.False(result.Section.Values["hasClientSecret"].GetValue<bool>());
        var stored = fixture.Site.GetOrCreate<GitHubAuthenticationSettings>();
        Assert.Null(stored.ClientSecret);
        Assert.Null(stored.ClientSecretSecretName);
    }

    [Fact]
    public async Task Update_SetAndClearSecret_IsRejected()
    {
        var fixture = new Fixture();
        var provider = fixture.Create("recaptcha");

        var result = await provider.UpdateAsync(new JsonObject { ["secretKey"] = "value", ["clearSecretKey"] = true });

        Assert.True(result.Errors.ContainsKey("secretKey"));
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Theory]
    [InlineData("{\"secretKey\":null}")]
    [InlineData("{\"secretKey\":\"  \"}")]
    [InlineData("{\"secretKey\":1}")]
    [InlineData("{\"siteKey\":true}")]
    public async Task Update_InvalidType_IsRejected(string json)
    {
        var fixture = new Fixture();

        var result = await fixture.Create("recaptcha").UpdateAsync(JsonNode.Parse(json).AsObject());

        Assert.NotEmpty(result.Errors);
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task Update_ReCaptchaKeys_AreTrimmedAndRefreshTheOptions()
    {
        var fixture = new Fixture();

        var result = await fixture.Create("recaptcha").UpdateAsync(new JsonObject { ["siteKey"] = " site ", ["secretKey"] = " secret " });

        Assert.Empty(result.Errors);
        var stored = fixture.Site.GetOrCreate<ReCaptchaSettings>();
        Assert.Equal("site", stored.SiteKey);
        Assert.Equal("secret", stored.SecretKey);
        fixture.Notifier.Verify(notifier => notifier.RequestUpdate<ReCaptchaSettings>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Update_ReverseProxyFlag_PreservesTheOthersAndReloads()
    {
        var fixture = new Fixture();
        fixture.Site.Put(new ReverseProxySettings { ForwardedHeaders = ForwardedHeaders.XForwardedHost });
        var provider = fixture.Create("reverse-proxy");

        var result = await provider.UpdateAsync(new JsonObject { ["enableXForwardedProto"] = true });

        Assert.True(result.ReloadRequested);
        Assert.False(provider.Descriptor.RequiresHttps);
        Assert.Equal(ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto, fixture.Site.GetOrCreate<ReverseProxySettings>().ForwardedHeaders);
        Assert.True(result.Section.Values["enableXForwardedHost"].GetValue<bool>());
        Assert.False(result.Section.Values["enableXForwardedFor"].GetValue<bool>());
    }

    [Fact]
    public async Task Get_ReverseProxyFromConfiguration_IsReadOnlyWithConfiguredValues()
    {
        var fixture = new Fixture { ReverseProxyOptions = new ReverseProxySettings { ForwardedHeaders = ForwardedHeaders.XForwardedFor, FromConfiguration = true } };

        var section = await fixture.Create("reverse-proxy").GetAsync();

        Assert.True(section.IsReadOnly);
        Assert.Equal("configuration", section.Source);
        Assert.NotEmpty(section.ReadOnlyReason);
        Assert.True(section.Values["enableXForwardedFor"].GetValue<bool>());
    }

    [Fact]
    public async Task Update_SmsDefaultProvider_MustBeEnabled()
    {
        var fixture = new Fixture();
        fixture.SmsProviders.TryAddProvider("Enabled", new SmsProviderTypeOptions(typeof(TwilioSmsProvider)) { IsEnabled = true });
        fixture.SmsProviders.TryAddProvider("Disabled", new SmsProviderTypeOptions(typeof(TwilioSmsProvider)) { IsEnabled = false });
        var provider = fixture.Create("sms");

        Assert.True((await provider.UpdateAsync(new JsonObject { ["defaultProvider"] = "Disabled" })).Errors.ContainsKey("defaultProvider"));
        Assert.True((await provider.UpdateAsync(new JsonObject { ["defaultProvider"] = "Unknown" })).Errors.ContainsKey("defaultProvider"));

        Assert.Empty((await provider.UpdateAsync(new JsonObject { ["defaultProvider"] = "Enabled" })).Errors);
        Assert.Equal("Enabled", fixture.Site.GetOrCreate<SmsSettings>().DefaultProviderName);

        Assert.Empty((await provider.UpdateAsync(new JsonObject { ["defaultProvider"] = null })).Errors);
        Assert.Null(fixture.Site.GetOrCreate<SmsSettings>().DefaultProviderName);
    }

    [Theory]
    [InlineData("{\"isEnabled\":true}", "phoneNumber")]
    [InlineData("{\"isEnabled\":true,\"phoneNumber\":\"invalid\",\"accountSid\":\"sid\",\"authToken\":\"token\"}", "phoneNumber")]
    [InlineData("{\"isEnabled\":true,\"phoneNumber\":\"+15550100\",\"authToken\":\"token\"}", "accountSid")]
    [InlineData("{\"isEnabled\":true,\"phoneNumber\":\"+15550100\",\"accountSid\":\"sid\"}", "authToken")]
    [InlineData("{\"phoneNumber\":\"+15550100\"}", "isEnabled")]
    public async Task Update_TwilioEnabledWithoutCredentials_IsRejected(string json, string property)
    {
        var fixture = new Fixture();

        var result = await fixture.Create("sms-twilio").UpdateAsync(JsonNode.Parse(json).AsObject());

        Assert.True(result.Errors.ContainsKey(property));
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task Update_TwilioEnabledThenDisabled_SetsAndClearsTheDefaultProvider()
    {
        var fixture = new Fixture();
        var provider = fixture.Create("sms-twilio");

        var enabled = await provider.UpdateAsync(new JsonObject { ["isEnabled"] = true, ["phoneNumber"] = "+15550100", ["accountSid"] = "sid", ["authToken"] = "test-token" });

        Assert.Empty(enabled.Errors);
        Assert.Equal(TwilioSmsProvider.TechnicalName, fixture.Site.GetOrCreate<SmsSettings>().DefaultProviderName);
        fixture.Notifier.Verify(notifier => notifier.RequestUpdate<TwilioOptions>(It.IsAny<string>()), Times.Once);

        var disabled = await provider.UpdateAsync(new JsonObject { ["isEnabled"] = false });

        Assert.Empty(disabled.Errors);
        Assert.Null(fixture.Site.GetOrCreate<SmsSettings>().DefaultProviderName);
        Assert.NotNull(fixture.Site.GetOrCreate<TwilioSettings>().AuthToken);
    }

    [Fact]
    public async Task Update_AdminListLayout_RefreshesTheListOptionsOnlyWhenItChanges()
    {
        var fixture = new Fixture();
        var provider = fixture.Create("admin");

        Assert.Empty((await provider.UpdateAsync(new JsonObject { ["showBreadcrumb"] = false })).Errors);
        fixture.Notifier.Verify(notifier => notifier.RequestUpdate<AdminListOptions>(It.IsAny<string>()), Times.Never);

        var result = await provider.UpdateAsync(new JsonObject { ["listLayout"] = "Grid" });

        Assert.Empty(result.Errors);
        Assert.Equal(AdminListLayout.Grid, fixture.Site.GetOrCreate<AdminSettings>().ListLayout);
        Assert.Equal("Grid", result.Section.Values["listLayout"].GetValue<string>());
        fixture.Notifier.Verify(notifier => notifier.RequestUpdate<AdminListOptions>(It.IsAny<string>()), Times.Once);

        Assert.Empty((await provider.UpdateAsync(new JsonObject { ["listLayout"] = null })).Errors);
        Assert.Null(fixture.Site.GetOrCreate<AdminSettings>().ListLayout);

        Assert.True((await provider.UpdateAsync(new JsonObject { ["menuBehavior"] = "1" })).Errors.ContainsKey("menuBehavior"));
    }

    [Fact]
    public async Task Update_FacebookInitParamsEmpty_KeepsTheCurrentOnes()
    {
        var fixture = new Fixture();
        fixture.Site.Put(new FacebookSettings { AppId = "id", FBInitParams = "xfbml: true" });
        var provider = fixture.Create("facebook");

        Assert.Empty((await provider.UpdateAsync(new JsonObject { ["fbInitParams"] = string.Empty })).Errors);
        Assert.Equal("xfbml: true", fixture.Site.GetOrCreate<FacebookSettings>().FBInitParams);

        Assert.True((await provider.UpdateAsync(new JsonObject { ["version"] = "3.2" })).Errors.ContainsKey("version"));
        Assert.Empty((await provider.UpdateAsync(new JsonObject { ["version"] = "v9.0" })).Errors);
    }

    [Fact]
    public async Task Update_TwitterWithoutAccessToken_IsRejected()
    {
        var fixture = new Fixture();

        var result = await fixture.Create("twitter").UpdateAsync(new JsonObject { ["consumerKey"] = "key" });

        Assert.True(result.Errors.ContainsKey("accessToken"));
        Assert.Null(fixture.Site.GetOrCreate<TwitterSettings>().ConsumerKey);
    }

    [Fact]
    public async Task Update_GoogleAnalyticsTrackingId_IsReadBack()
    {
        var fixture = new Fixture();

        var result = await fixture.Create("google-analytics").UpdateAsync(new JsonObject { ["trackingId"] = "G-TEST" });

        Assert.False(result.ReloadRequested);
        Assert.Equal("G-TEST", result.Section.Values["trackingId"].GetValue<string>());
        Assert.Equal("G-TEST", fixture.Site.GetOrCreate<GoogleAnalyticsSettings>().TrackingID);
        fixture.Release.Verify(release => release.RequestRelease(), Times.Never);
    }

    private sealed class Fixture
    {
        public SiteSettings Site { get; } = new() { IsReadOnly = false };
        public Mock<ISiteService> Service { get; } = new();
        public Mock<IShellReleaseManager> Release { get; } = new();
        public Mock<IOptionsUpdateNotifier> Notifier { get; } = new();
        public EphemeralDataProtectionProvider Protection { get; } = new();
        public SmsProviderOptions SmsProviders { get; } = new();
        public ReverseProxySettings ReverseProxyOptions { get; init; } = new();

        public Fixture()
        {
            Service.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(Site);
            Service.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(Site);
            Notifier.Setup(notifier => notifier.RequestUpdate<TwilioOptions>(It.IsAny<string>())).Returns(Notifier.Object);
            Notifier.Setup(notifier => notifier.RequestUpdate<SmsProviderOptions>(It.IsAny<string>())).Returns(Notifier.Object);
        }

        // The values that make a section valid, so that a test only exercises the property it is about.
        public static JsonObject ValidValues(string name, JsonObject values)
        {
            var required = name switch
            {
                "google-analytics" => new JsonObject { ["trackingId"] = "G-TEST" },
                "google-tag-manager" => new JsonObject { ["containerId"] = "GTM-TEST" },
                "google-authentication" or "github-authentication" => new JsonObject { ["clientId"] = "id" },
                "microsoft-account" or "facebook" => new JsonObject { ["appId"] = "id" },
                "microsoft-entra-id" => new JsonObject { ["displayName"] = "Name", ["appId"] = "id", ["tenantId"] = "tenant" },
                "twitter" => new JsonObject { ["consumerKey"] = "key", ["accessToken"] = "test-token" },
                "sms-twilio" => new JsonObject { ["isEnabled"] = true, ["phoneNumber"] = "+15550100", ["accountSid"] = "sid", ["authToken"] = "test-token" },
                _ => [],
            };

            foreach (var entry in values)
            {
                required[entry.Key] = entry.Value?.DeepClone();
            }

            return required;
        }

        public ISiteSettingsSectionProvider Create(string name)
        {
            var site = Service.Object;
            var release = Release.Object;
            var localizer = new StringLocalizer<ISiteSettingsSectionProvider>(new global::OrchardCore.Localization.NullStringLocalizerFactory());
            var phone = new Mock<IPhoneFormatValidator>();
            phone.Setup(value => value.IsValid(It.IsAny<string>())).Returns((string value) => value.StartsWith('+'));

            return name switch
            {
                "recaptcha" => new ReCaptchaSettingsSectionProvider(Notifier.Object, site, release, localizer),
                "reverse-proxy" => new ReverseProxySettingsSectionProvider(Mock.Of<IOptionsSnapshot<ReverseProxySettings>>(options => options.Value == ReverseProxyOptions), site, release, localizer),
                "google-analytics" => new GoogleAnalyticsSettingsSectionProvider(site, release, localizer),
                "google-tag-manager" => new GoogleTagManagerSettingsSectionProvider(site, release, localizer),
                "google-authentication" => new GoogleAuthenticationSettingsSectionProvider(Protection, site, release, localizer),
                "github-authentication" => new GitHubAuthenticationSettingsSectionProvider(Protection, site, release, localizer),
                "microsoft-account" => new MicrosoftAccountSettingsSectionProvider(Protection, site, release, localizer),
                "microsoft-entra-id" => new MicrosoftEntraIdSettingsSectionProvider(site, release, localizer),
                "twitter" => new TwitterSettingsSectionProvider(Protection, site, release, localizer),
                "twitter-signin" => new TwitterSigninSettingsSectionProvider(site, release, localizer),
                "facebook" => new FacebookSettingsSectionProvider(Protection, site, release, localizer),
                "facebook-login" => new FacebookLoginSettingsSectionProvider(site, release, localizer),
                "facebook-pixel" => new FacebookPixelSettingsSectionProvider(Protection, site, release, localizer),
                "sms" => new SmsSettingsSectionProvider(Mock.Of<IOptionsMonitor<SmsProviderOptions>>(options => options.CurrentValue == SmsProviders), site, release, localizer),
                "sms-twilio" => new TwilioSettingsSectionProvider(Notifier.Object, phone.Object, Protection, site, release, localizer),
                "admin" => new AdminSettingsSectionProvider(Notifier.Object, site, release, localizer),
                _ => throw new ArgumentOutOfRangeException(nameof(name)),
            };
        }
    }
}
