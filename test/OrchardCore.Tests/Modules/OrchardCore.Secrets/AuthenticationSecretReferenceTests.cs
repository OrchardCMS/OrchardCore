using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Login.Configuration;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.Google.Authentication.Configuration;
using OrchardCore.Google.Authentication.Services;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.Configuration;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.Signin.Configuration;
using OrchardCore.Twitter.Signin.Services;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class AuthenticationSecretReferenceTests
{
    private readonly SiteSettings _site = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<ISecretManager> _manager = new();
    private readonly IServiceProvider _services;

    public AuthenticationSecretReferenceTests()
    {
        _siteService.Setup(s => s.GetSiteSettingsAsync()).ReturnsAsync(_site);
        _siteService.Setup(s => s.LoadSiteSettingsAsync()).ReturnsAsync(_site);
        _manager.Setup(m => m.GetSecretAsync<TextSecret>("reference")).ReturnsAsync(new TextSecret { Text = "resolved" });
        _services = new ServiceCollection().AddSingleton(_manager.Object).BuildServiceProvider();
    }

    [Fact]
    public async Task Facebook_UsesReferenceThroughValidationProjectionAndLogin()
    {
        _site.Put(new FacebookSettings { AppId = "id", AppSecretSecretName = "reference" });
        var service = new FacebookService(_siteService.Object, Localizer<FacebookService>());
        Assert.Empty(service.ValidateSettings(await service.GetSettingsAsync()));
        var settings = new FacebookSettings();
        new FacebookSettingsConfiguration(service).Configure(settings);
        Assert.Equal("reference", settings.AppSecretSecretName);

        var login = new Mock<IFacebookLoginService>();
        login.Setup(s => s.GetSettingsAsync()).ReturnsAsync(new FacebookLoginSettings());
        login.Setup(s => s.ValidateSettingsAsync(It.IsAny<FacebookLoginSettings>())).ReturnsAsync(Array.Empty<ValidationResult>());
        var configuration = new FacebookLoginConfiguration(Options.Create(settings), login.Object,
            new EphemeralDataProtectionProvider(), _services, NullLogger<FacebookLoginConfiguration>.Instance);
        var authentication = new AuthenticationOptions();
        configuration.Configure(authentication);
        Assert.Contains(authentication.Schemes, s => s.Name == FacebookDefaults.AuthenticationScheme);
        var options = new FacebookOptions();
        configuration.Configure(FacebookDefaults.AuthenticationScheme, options);
        Assert.Equal("resolved", options.AppSecret);
    }

    [Fact]
    public async Task Google_PersistsReferenceAndResolvesProjectedOptions()
    {
        var service = new GoogleAuthenticationService(_siteService.Object, Localizer<GoogleAuthenticationService>());
        await service.UpdateSettingsAsync(new GoogleAuthenticationSettings { ClientID = "id", ClientSecretSecretName = "reference" });
        Assert.Empty(service.ValidateSettings(await service.GetSettingsAsync()));
        var settings = new GoogleAuthenticationSettings();
        new GoogleAuthenticationSettingsConfiguration(service).Configure(settings);
        Assert.Equal("reference", settings.ClientSecretSecretName);
        var configuration = new GoogleOptionsConfiguration(Options.Create(settings), new EphemeralDataProtectionProvider(),
            _services, NullLogger<GoogleOptionsConfiguration>.Instance);
        var authentication = new AuthenticationOptions();
        configuration.Configure(authentication);
        Assert.Contains(authentication.Schemes, s => s.Name == GoogleDefaults.AuthenticationScheme);
        var options = new GoogleOptions();
        configuration.Configure(GoogleDefaults.AuthenticationScheme, options);
        Assert.Equal("resolved", options.ClientSecret);
    }

    [Fact]
    public async Task Microsoft_PersistsReferenceAndResolvesProjectedOptions()
    {
        var service = new MicrosoftAccountService(_siteService.Object, Localizer<MicrosoftAccountService>());
        await service.UpdateSettingsAsync(new MicrosoftAccountSettings { AppId = "id", AppSecretSecretName = "reference" });
        Assert.Empty(service.ValidateSettings(await service.GetSettingsAsync()));
        var settings = new MicrosoftAccountSettings();
        new MicrosoftAccountSettingsConfiguration(service).Configure(settings);
        Assert.Equal("reference", settings.AppSecretSecretName);
        var configuration = new MicrosoftAccountOptionsConfiguration(Options.Create(settings), _services,
            new EphemeralDataProtectionProvider(), NullLogger<MicrosoftAccountOptionsConfiguration>.Instance);
        var authentication = new AuthenticationOptions();
        configuration.Configure(authentication);
        Assert.Contains(authentication.Schemes, s => s.Name == MicrosoftAccountDefaults.AuthenticationScheme);
        var options = new MicrosoftAccountOptions();
        configuration.Configure(MicrosoftAccountDefaults.AuthenticationScheme, options);
        Assert.Equal("resolved", options.ClientSecret);
    }

    [Fact]
    public async Task Twitter_PersistsBothReferencesAndResolvesSigninOptions()
    {
        var service = new TwitterSettingsService(_siteService.Object, Localizer<TwitterSettingsService>());
        await service.UpdateSettingsAsync(new TwitterSettings
        {
            ConsumerKey = "id",
            AccessToken = "token",
            ConsumerSecretSecretName = "reference",
            AccessTokenSecretSecretName = "reference",
        });
        Assert.Empty(service.ValidateSettings(await service.GetSettingsAsync()));
        var settings = new TwitterSettings();
        new TwitterSettingsConfiguration(service).Configure(settings);
        Assert.Equal("reference", settings.ConsumerSecretSecretName);
        Assert.Equal("reference", settings.AccessTokenSecretSecretName);
        var signin = new Mock<ITwitterSigninService>();
        signin.Setup(s => s.GetSettingsAsync()).ReturnsAsync(new TwitterSigninSettings());
        var configuration = new TwitterOptionsConfiguration(service, signin.Object, new EphemeralDataProtectionProvider(),
            _services, new HttpContextAccessor(), new ShellSettings(), NullLogger<TwitterOptionsConfiguration>.Instance);
        var authentication = new AuthenticationOptions();
        configuration.Configure(authentication);
        Assert.Contains(authentication.Schemes, s => s.Name == TwitterDefaults.AuthenticationScheme);
        var options = new TwitterOptions();
        configuration.Configure(TwitterDefaults.AuthenticationScheme, options);
        Assert.Equal("resolved", options.ConsumerSecret);
    }

    [Fact]
    public void Twilio_ResolvesReferenceInOptions()
    {
        _site.Put(new TwilioSettings
        {
            IsEnabled = true,
            AccountSID = "account",
            PhoneNumber = "+15555555555",
            AuthTokenSecretName = "reference",
        });
        var configuration = new TwilioOptionsConfiguration(_siteService.Object, new EphemeralDataProtectionProvider(), _services,
            NullLogger<TwilioOptionsConfiguration>.Instance);
        var options = new TwilioOptions();

        configuration.Configure(options);

        Assert.True(options.IsEnabled);
        Assert.Equal("resolved", options.AuthToken);
    }

    private static IStringLocalizer<T> Localizer<T>()
    {
        var localizer = new Mock<IStringLocalizer<T>>();
        localizer.Setup(s => s[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        return localizer.Object;
    }
}
