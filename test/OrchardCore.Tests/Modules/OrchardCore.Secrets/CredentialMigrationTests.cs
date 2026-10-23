using System.Security.Cryptography;
using System.Text.Json.Nodes;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class CredentialMigrationTests
{
    private static readonly string[] s_integrations =
    [
        "GitHub",
        "Facebook",
        "Google",
        "Microsoft",
        "TwitterConsumer",
        "TwitterAccessToken",
        "Twilio",
        "AzureSms",
        "AzureEmail",
        "Smtp",
        "OpenIdClient",
        "AzureAI",
    ];

    public static TheoryData<string> Integrations => new(s_integrations);

    [Theory]
    [InlineData("GitHub.ClientSecret", global::OrchardCore.GitHub.GitHubConstants.SecretNames.ClientSecret)]
    [InlineData("Facebook.AppSecret", global::OrchardCore.Facebook.FacebookConstants.SecretNames.AppSecret)]
    [InlineData("Google.ClientSecret", global::OrchardCore.Google.GoogleConstants.SecretNames.ClientSecret)]
    [InlineData("MicrosoftAccount.AppSecret", global::OrchardCore.Microsoft.Authentication.MicrosoftAuthenticationConstants.SecretNames.AppSecret)]
    [InlineData("Twitter.ConsumerSecret", global::OrchardCore.Twitter.TwitterConstants.SecretNames.ConsumerSecret)]
    [InlineData("Twitter.AccessTokenSecret", global::OrchardCore.Twitter.TwitterConstants.SecretNames.AccessTokenSecret)]
    [InlineData("OpenIdClient.ClientSecret", global::OrchardCore.OpenId.OpenIdConstants.SecretNames.ClientSecret)]
    [InlineData("OpenId.SigningCertificate", global::OrchardCore.OpenId.OpenIdConstants.SecretNames.SigningCertificate)]
    [InlineData("OpenId.EncryptionCertificate", global::OrchardCore.OpenId.OpenIdConstants.SecretNames.EncryptionCertificate)]
    [InlineData("AzureAISearch.ApiKey", global::OrchardCore.AzureAI.AzureAISearchConstants.SecretNames.ApiKey)]
    [InlineData("Smtp.Password", global::OrchardCore.Email.Smtp.Services.SmtpOptionsConfiguration.PasswordSecretName)]
    public void DefaultSecretNames_PreservePersistedValues(string expected, string actual)
    {
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Integrations))]
    public async Task Edit_ListsOnlyCredentialsKeptInSettings(string integration)
    {
        var scenario = GetScenario(integration);
        var provider = new EphemeralDataProtectionProvider();
        var encrypted = provider.CreateProtector(scenario.Purpose).Protect("credential");

        // Settings are read once per site, so each case uses its own one.
        Assert.Null(await new DriverTest(scenario, provider).EditAsync());

        var kept = new DriverTest(scenario, provider);
        kept.Site.Properties[scenario.Settings] = new JsonObject { [scenario.LegacyProperty] = encrypted };
        Assert.NotNull(await kept.EditAsync());

        var referenced = new DriverTest(scenario, provider);
        referenced.Site.Properties[scenario.Settings] = new JsonObject
        {
            [scenario.LegacyProperty] = encrypted,
            [scenario.ReferenceProperty] = "Existing",
        };
        Assert.Null(await referenced.EditAsync());
    }

    [Theory]
    [MemberData(nameof(Integrations))]
    public async Task Update_WhenSelected_MovesTheCredentialToTheSelectedStore(string integration)
    {
        var scenario = GetScenario(integration);
        var provider = new EphemeralDataProtectionProvider();
        var test = new DriverTest(scenario, provider);
        test.Site.Properties[scenario.Settings] = new JsonObject
        {
            [scenario.LegacyProperty] = provider.CreateProtector(scenario.Purpose).Protect("credential"),
        };

        var migration = await test.UpdateAsync(migrate: true, " Moved ");

        Assert.True(Assert.Single(migration.Results).Succeeded);
        Assert.Null(test.Site.Properties[scenario.Settings][scenario.LegacyProperty]?.GetValue<string>());
        Assert.Equal("Moved", test.Site.Properties[scenario.Settings][scenario.ReferenceProperty].GetValue<string>());
        test.SecretManager.Verify(m => m.SaveSecretAsync("Moved", It.Is<TextSecret>(s => s.Text == "credential"), "Vault", It.IsAny<SecretSaveOptions>()), Times.Once);
        test.SiteService.Verify(s => s.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Integrations))]
    public async Task Update_WhenNotSelected_KeepsTheCredential(string integration)
    {
        var scenario = GetScenario(integration);
        var provider = new EphemeralDataProtectionProvider();
        var test = new DriverTest(scenario, provider);
        var encrypted = provider.CreateProtector(scenario.Purpose).Protect("credential");
        test.Site.Properties[scenario.Settings] = new JsonObject { [scenario.LegacyProperty] = encrypted };

        var migration = await test.UpdateAsync(migrate: false, "Moved");

        Assert.Empty(migration.Results);
        Assert.Equal(encrypted, test.Site.Properties[scenario.Settings][scenario.LegacyProperty].GetValue<string>());
        test.SiteService.Verify(s => s.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(Integrations))]
    public async Task Update_WhenDecryptionFails_KeepsTheCredential(string integration)
    {
        var scenario = GetScenario(integration);
        var failingProtector = new Mock<IDataProtector>();
        failingProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Throws(new CryptographicException("Key missing"));
        var failingProvider = new Mock<IDataProtectionProvider>();
        failingProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(failingProtector.Object);
        var test = new DriverTest(scenario, failingProvider.Object);
        test.Site.Properties[scenario.Settings] = new JsonObject { [scenario.LegacyProperty] = "encrypted" };

        var migration = await test.UpdateAsync(migrate: true, "Moved");

        Assert.Equal(SecretMigrationError.DecryptionFailed, Assert.Single(migration.Results).Error);
        Assert.Equal("encrypted", test.Site.Properties[scenario.Settings][scenario.LegacyProperty].GetValue<string>());
        Assert.Null(test.Site.Properties[scenario.Settings][scenario.ReferenceProperty]);
        test.SiteService.Verify(s => s.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        test.SecretManager.Verify(m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<TextSecret>(), It.IsAny<string>(), It.IsAny<SecretSaveOptions>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(Integrations))]
    public async Task Update_WhenTheStoreFails_KeepsTheCredential(string integration)
    {
        var scenario = GetScenario(integration);
        var provider = new EphemeralDataProtectionProvider();
        var test = new DriverTest(scenario, provider);
        var encrypted = provider.CreateProtector(scenario.Purpose).Protect("credential");
        test.Site.Properties[scenario.Settings] = new JsonObject { [scenario.LegacyProperty] = encrypted };
        test.SecretManager.Setup(m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<TextSecret>(), It.IsAny<string>(), It.IsAny<SecretSaveOptions>()))
            .ThrowsAsync(new InvalidOperationException("Store unavailable"));

        var migration = await test.UpdateAsync(migrate: true, "Moved");

        Assert.Equal(SecretMigrationError.StoreFailed, Assert.Single(migration.Results).Error);
        Assert.Equal(encrypted, test.Site.Properties[scenario.Settings][scenario.LegacyProperty].GetValue<string>());
        test.SiteService.Verify(s => s.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task Update_EachDriverBindsItsOwnFields()
    {
        var prefixes = new List<string>();

        foreach (var integration in s_integrations)
        {
            var scenario = GetScenario(integration);
            var provider = new EphemeralDataProtectionProvider();
            var test = new DriverTest(scenario, provider);
            test.Site.Properties[scenario.Settings] = new JsonObject
            {
                [scenario.LegacyProperty] = provider.CreateProtector(scenario.Purpose).Protect("credential"),
            };

            await test.UpdateAsync(migrate: false, "Moved");
            prefixes.AddRange(test.BoundPrefixes);
        }

        Assert.Equal(prefixes.Count, prefixes.Distinct(StringComparer.Ordinal).Count());
    }

    private static MigrationScenario GetScenario(string integration) => integration switch
    {
        "GitHub" => new(typeof(global::OrchardCore.GitHub.Drivers.GitHubSecretMigrationDisplayDriver), "GitHubAuthenticationSettings", "ClientSecret", "ClientSecretSecretName", global::OrchardCore.GitHub.GitHubConstants.Features.GitHubAuthentication, null),
        "Facebook" => new(typeof(global::OrchardCore.Facebook.Drivers.FacebookSecretMigrationDisplayDriver), "FacebookSettings", "AppSecret", "AppSecretSecretName", global::OrchardCore.Facebook.FacebookConstants.Features.Core, null),
        "Google" => new(typeof(global::OrchardCore.Google.Authentication.Drivers.GoogleSecretMigrationDisplayDriver), "GoogleAuthenticationSettings", "ClientSecret", "ClientSecretSecretName", global::OrchardCore.Google.GoogleConstants.Features.GoogleAuthentication, null),
        "Microsoft" => new(typeof(global::OrchardCore.Microsoft.Authentication.Drivers.MicrosoftAccountSecretMigrationDisplayDriver), "MicrosoftAccountSettings", "AppSecret", "AppSecretSecretName", global::OrchardCore.Microsoft.Authentication.MicrosoftAuthenticationConstants.Features.MicrosoftAccount, null),
        "TwitterConsumer" => new(typeof(global::OrchardCore.Twitter.Drivers.TwitterSecretMigrationDisplayDriver), "TwitterSettings", "ConsumerSecret", "ConsumerSecretSecretName", global::OrchardCore.Twitter.TwitterConstants.Features.Twitter, ".ConsumerSecret"),
        "TwitterAccessToken" => new(typeof(global::OrchardCore.Twitter.Drivers.TwitterSecretMigrationDisplayDriver), "TwitterSettings", "AccessTokenSecret", "AccessTokenSecretSecretName", global::OrchardCore.Twitter.TwitterConstants.Features.Twitter, ".AccessTokenSecret"),
        "Smtp" => new(typeof(global::OrchardCore.Email.Smtp.Drivers.SmtpSecretMigrationDisplayDriver), "SmtpSettings", "Password", "PasswordSecretName", global::OrchardCore.Email.Smtp.Services.SmtpOptionsConfiguration.ProtectorName, null),
        "Twilio" => new(typeof(global::OrchardCore.Sms.Drivers.TwilioSecretMigrationDisplayDriver), "TwilioSettings", "AuthToken", "AuthTokenSecretName", global::OrchardCore.Sms.Services.TwilioSmsProvider.ProtectorName, null),
        "AzureSms" => new(typeof(global::OrchardCore.Sms.Azure.Drivers.AzureSmsSecretMigrationDisplayDriver), "AzureSmsSettings", "ConnectionString", "ConnectionStringSecretName", global::OrchardCore.Sms.Azure.Services.AzureSmsOptionsConfiguration.ProtectorName, null),
        "AzureEmail" => new(typeof(global::OrchardCore.Azure.Email.Drivers.AzureEmailSecretMigrationDisplayDriver), "AzureEmailSettings", "ConnectionString", "ConnectionStringSecretName", global::OrchardCore.Email.Services.AzureEmailOptionsConfiguration.ProtectorName, null),
        "AzureAI" => new(typeof(global::OrchardCore.AzureAI.Drivers.AzureAISearchSecretMigrationDisplayDriver), "AzureAISearchDefaultSettings", "ApiKey", "ApiKeySecretName", global::OrchardCore.AzureAI.Services.AzureAISearchDefaultOptionsConfigurations.ProtectorName, null),
        "OpenIdClient" => new(typeof(global::OrchardCore.OpenId.Drivers.OpenIdClientSecretMigrationDisplayDriver), "OpenIdClientSettings", "ClientSecret", "ClientSecretSecretName", "OpenIdClientConfiguration", null),
        _ => throw new ArgumentOutOfRangeException(nameof(integration)),
    };

    /// <param name="ItemPrefix">The prefix of the migrated item when the driver contributes several credentials.</param>
    private sealed record MigrationScenario(Type Driver, string Settings, string LegacyProperty, string ReferenceProperty, string Purpose, string ItemPrefix);

    private sealed class DriverTest
    {
        private readonly MigrationScenario _scenario;
        private readonly IDisplayDriver<SecretMigration> _driver;

        public DriverTest(MigrationScenario scenario, IDataProtectionProvider dataProtectionProvider)
        {
            _scenario = scenario;

            SiteService.Setup(s => s.GetSiteSettingsAsync()).ReturnsAsync(Site);
            SiteService.Setup(s => s.LoadSiteSettingsAsync()).ReturnsAsync(Site);
            SecretManager.Setup(m => m.GetSecretInfosAsync()).ReturnsAsync(
                [new SecretInfo { Name = "Existing", Store = "Vault", Type = typeof(TextSecret).FullName }]);

            var authorization = new Mock<IAuthorizationService>();
            authorization.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var services = new ServiceCollection()
                .AddLocalization()
                .AddLogging()
                .AddSingleton(SiteService.Object)
                .AddSingleton(SecretManager.Object)
                .AddSingleton(dataProtectionProvider)
                .AddSingleton(authorization.Object)
                .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() })
                .AddSingleton<global::OrchardCore.OpenId.Services.IOpenIdClientService, global::OrchardCore.OpenId.Services.OpenIdClientService>()
                .BuildServiceProvider();

            _driver = (IDisplayDriver<SecretMigration>)ActivatorUtilities.CreateInstance(services, scenario.Driver);
        }

        public SiteSettings Site { get; } = new();

        public Mock<ISiteService> SiteService { get; } = new();

        public Mock<ISecretManager> SecretManager { get; } = new();

        public List<string> BoundPrefixes { get; } = [];

        public Task<IDisplayResult> EditAsync()
            => _driver.BuildEditorAsync(new SecretMigration(), new BuildEditorContext(new Shape(), string.Empty, false, string.Empty, null, null, null));

        public async Task<SecretMigration> UpdateAsync(bool migrate, string secretName)
        {
            var updater = new Mock<IUpdateModel>();
            updater.SetupGet(u => u.ModelState).Returns(new global::Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());
            updater.Setup(u => u.TryUpdateModelAsync(It.IsAny<SecretMigrationItemViewModel>(), It.IsAny<string>()))
                .Callback<SecretMigrationItemViewModel, string>((model, prefix) =>
                {
                    // Only the item of the tested credential is selected when a driver contributes several.
                    BoundPrefixes.Add(prefix);
                    var selected = _scenario.ItemPrefix is null || prefix.EndsWith(_scenario.ItemPrefix, StringComparison.Ordinal);
                    model.Migrate = migrate && selected;
                    model.SecretName = secretName;
                })
                .ReturnsAsync(true);

            var migration = new SecretMigration { Store = "Vault" };
            await _driver.UpdateEditorAsync(migration, new UpdateEditorContext(new Shape(), string.Empty, false, string.Empty, null, null, updater.Object));

            // A secret store can commit in its own scope, so no driver may change settings before every secret is saved.
            SiteService.Verify(s => s.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
            await migration.UpdateSettingsAsync();

            return migration;
        }
    }
}
