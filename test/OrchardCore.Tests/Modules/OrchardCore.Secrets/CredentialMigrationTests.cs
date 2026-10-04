using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class CredentialMigrationTests
{
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
    public void DefaultSecretNames_PreservePersistedValues(string expected, string actual)
    {
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("GitHub")]
    [InlineData("Facebook")]
    [InlineData("Google")]
    [InlineData("Microsoft")]
    [InlineData("Twitter")]
    [InlineData("Twilio")]
    [InlineData("AzureSms")]
    [InlineData("AzureEmail")]
    [InlineData("Smtp")]
    [InlineData("OpenIdClient")]
    [InlineData("AzureAI")]
    public async Task FailedDecryption_PreservesSourceAndCanBeRetried(string integration)
    {
        var scenario = GetScenario(integration);
        var provider = new EphemeralDataProtectionProvider();
        var encrypted = provider.CreateProtector(scenario.Purpose).Protect("credential");
        var site = new SiteSettings();
        site.Properties[scenario.Settings] = new JsonObject { [scenario.LegacyProperty] = encrypted };
        var siteService = new Mock<ISiteService>();
        siteService.Setup(s => s.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var manager = new Mock<ISecretManager>();
        var failingProvider = new Mock<IDataProtectionProvider>();
        var failingProtector = new Mock<IDataProtector>();
        failingProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Throws(new CryptographicException("Key missing"));
        failingProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(failingProtector.Object);

        using var services = CreateServices(siteService.Object, manager.Object, failingProvider.Object);
        var migration = ActivatorUtilities.CreateInstance(services, scenario.Migration);
        await Assert.ThrowsAsync<CryptographicException>(() => InvokeMigrationAsync(migration, "CreateAsync"));
        Assert.Equal(encrypted, site.Properties[scenario.Settings][scenario.LegacyProperty].GetValue<string>());
        Assert.Null(site.Properties[scenario.ReferenceSettings]?[scenario.ReferenceProperty]);
        siteService.Verify(s => s.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        Assert.Empty(manager.Invocations);

        using var retryServices = CreateServices(siteService.Object, manager.Object, provider);
        migration = ActivatorUtilities.CreateInstance(retryServices, scenario.Migration);
        Assert.Equal(1, await InvokeMigrationAsync(migration, "CreateAsync"));
        Assert.Null(site.Properties[scenario.Settings][scenario.LegacyProperty]);
        Assert.Equal(scenario.SecretName, site.Properties[scenario.ReferenceSettings][scenario.ReferenceProperty].GetValue<string>());
        Assert.Contains(manager.Invocations, call => call.Method.Name == nameof(ISecretManager.SaveSecretAsync) &&
            call.Arguments[1] is TextSecret text && text.Text == "credential");
    }

    [Theory]
    [InlineData("GitHub")]
    [InlineData("Facebook")]
    [InlineData("Google")]
    [InlineData("Microsoft")]
    [InlineData("Twitter")]
    [InlineData("Twilio")]
    [InlineData("AzureSms")]
    [InlineData("AzureEmail")]
    [InlineData("Smtp")]
    [InlineData("OpenIdClient")]
    [InlineData("AzureAI")]
    public async Task FailedSave_DoesNotClearLegacyCredential(string integration)
    {
        var scenario = GetScenario(integration);
        var provider = new EphemeralDataProtectionProvider();
        var encrypted = provider.CreateProtector(scenario.Purpose).Protect("credential");
        var site = new SiteSettings();
        site.Properties[scenario.Settings] = new JsonObject { [scenario.LegacyProperty] = encrypted };
        var siteService = new Mock<ISiteService>();
        siteService.Setup(s => s.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var manager = new Mock<ISecretManager>();
        manager.Setup(m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<TextSecret>(), It.IsAny<SecretSaveOptions>()))
            .ThrowsAsync(new InvalidOperationException("Store unavailable"));
        using var services = CreateServices(siteService.Object, manager.Object, provider);
        var migration = ActivatorUtilities.CreateInstance(services, scenario.Migration);

        await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeMigrationAsync(migration, "CreateAsync"));

        Assert.Equal(encrypted, site.Properties[scenario.Settings][scenario.LegacyProperty].GetValue<string>());
        siteService.Verify(s => s.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    private static ServiceProvider CreateServices(ISiteService site, ISecretManager manager, IDataProtectionProvider protection) =>
        new ServiceCollection().AddLogging().AddSingleton(site).AddSingleton(manager).AddSingleton(protection).BuildServiceProvider();

    private static Task<int> InvokeMigrationAsync(object migration, string method) =>
        (Task<int>)migration.GetType().GetMethod(method).Invoke(migration, null);

    private static MigrationScenario GetScenario(string integration) => integration switch
    {
        "GitHub" => new(typeof(global::OrchardCore.GitHub.Migrations), "GitHubAuthenticationSettings", "ClientSecret", "GitHubAuthenticationSettings", "ClientSecretSecretName", global::OrchardCore.GitHub.GitHubConstants.Features.GitHubAuthentication, "GitHub.ClientSecret"),
        "Facebook" => new(typeof(global::OrchardCore.Facebook.Migrations), "FacebookSettings", "AppSecret", "FacebookSettings", "AppSecretSecretName", global::OrchardCore.Facebook.FacebookConstants.Features.Core, "Facebook.AppSecret"),
        "Google" => new(typeof(global::OrchardCore.Google.Migrations), "GoogleAuthenticationSettings", "ClientSecret", "GoogleAuthenticationSettings", "ClientSecretSecretName", global::OrchardCore.Google.GoogleConstants.Features.GoogleAuthentication, "Google.ClientSecret"),
        "Microsoft" => new(typeof(global::OrchardCore.Microsoft.Authentication.Migrations), "MicrosoftAccountSettings", "AppSecret", "MicrosoftAccountSettings", "AppSecretSecretName", global::OrchardCore.Microsoft.Authentication.MicrosoftAuthenticationConstants.Features.MicrosoftAccount, "MicrosoftAccount.AppSecret"),
        "Twitter" => new(typeof(global::OrchardCore.Twitter.Migrations), "TwitterSettings", "ConsumerSecret", "TwitterSettings", "ConsumerSecretSecretName", global::OrchardCore.Twitter.TwitterConstants.Features.Twitter, "Twitter.ConsumerSecret"),
        "Twilio" => new(typeof(global::OrchardCore.Sms.Migrations), "TwilioSettings", "AuthToken", "TwilioSettings", "AuthTokenSecretName", global::OrchardCore.Sms.Services.TwilioSmsProvider.ProtectorName, "Twilio.AuthToken"),
        "AzureSms" => new(typeof(global::OrchardCore.Sms.Azure.Migrations), "AzureSmsSettings", "ConnectionString", "AzureSmsSettings", "ConnectionStringSecretName", global::OrchardCore.Sms.Azure.Services.AzureSmsOptionsConfiguration.ProtectorName, "AzureSms.ConnectionString"),
        "AzureEmail" => new(typeof(global::OrchardCore.Email.Azure.Migrations), "AzureEmailSettings", "ConnectionString", "AzureEmailSettings", "ConnectionStringSecretName", global::OrchardCore.Email.Services.AzureEmailOptionsConfiguration.ProtectorName, "AzureEmail.ConnectionString"),
        "Smtp" => new(typeof(global::OrchardCore.Email.Smtp.Secrets.Migrations), "SmtpSettings", "Password", "SmtpSecretSettings", "PasswordSecretName", "SmtpSettingsConfiguration", "Smtp.Password"),
        "OpenIdClient" => new(typeof(global::OrchardCore.OpenId.Migrations.ClientSecretsMigration), "OpenIdClientSettings", "ClientSecret", "OpenIdClientSettings", "ClientSecretSecretName", "OpenIdClientConfiguration", "OpenIdClient.ClientSecret"),
        "AzureAI" => new(typeof(global::OrchardCore.AzureAI.Migrations.ApiKeySecretsMigration), "AzureAISearchDefaultSettings", "ApiKey", "AzureAISearchDefaultSettings", "ApiKeySecretName", global::OrchardCore.AzureAI.Services.AzureAISearchDefaultOptionsConfigurations.ProtectorName, "AzureAISearch.ApiKey"),
        _ => throw new ArgumentOutOfRangeException(nameof(integration)),
    };

    private sealed record MigrationScenario(Type Migration, string Settings, string LegacyProperty, string ReferenceSettings,
        string ReferenceProperty, string Purpose, string SecretName);
}
