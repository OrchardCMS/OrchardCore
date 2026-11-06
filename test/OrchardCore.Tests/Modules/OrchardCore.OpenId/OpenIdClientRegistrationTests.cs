using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Client.WebIntegration;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdClientRegistrationTests
{
    [Fact]
    public void Configure_NoRegistration_DoesNotRegisterTheClientHandler()
    {
        using var services = BuildClientServices();

        var authenticationOptions = services.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        Assert.DoesNotContain(authenticationOptions.Schemes, scheme => scheme.Name == OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [Fact]
    public void Configure_SeveralRegistrations_RegistersASchemeAndTheEndpointsOfEach()
    {
        using var services = BuildClientServices(
            CreateRegistration(OpenIdConstants.Client.DefaultProviderName, "https://idp1.example.com/", "signin-oidc", "signout-callback-oidc"),
            CreateRegistration("Partner", "https://idp2.example.com/", "signin-oidc-partner", "signout-callback-oidc-partner"));

        var authenticationOptions = services.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        var clientOptions = services.GetRequiredService<IOptionsMonitor<OpenIddictClientOptions>>().CurrentValue;

        Assert.Contains(authenticationOptions.Schemes, scheme => scheme.Name == OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
        Assert.Contains(authenticationOptions.Schemes, scheme => scheme.Name == OpenIdConstants.Client.DefaultProviderName && scheme.DisplayName == "OpenIdConnect IdP");
        Assert.Contains(authenticationOptions.Schemes, scheme => scheme.Name == "Partner" && scheme.DisplayName == "Partner IdP");

        Assert.Equal(2, clientOptions.Registrations.Count);
        Assert.Equal(["signin-oidc", "signin-oidc-partner"], clientOptions.RedirectionEndpointUris.Select(uri => uri.OriginalString));
        Assert.Equal(["signout-callback-oidc", "signout-callback-oidc-partner"], clientOptions.PostLogoutRedirectionEndpointUris.Select(uri => uri.OriginalString));
    }

    [Fact]
    public void Configure_RegistrationsSharingAProviderName_KeepsTheFirstOne()
    {
        using var services = BuildClientServices(
            CreateRegistration("Partner", "https://idp1.example.com/", "signin-oidc-1", "signout-callback-oidc-1"),
            CreateRegistration("partner", "https://idp2.example.com/", "signin-oidc-2", "signout-callback-oidc-2"));

        var clientOptions = services.GetRequiredService<IOptionsMonitor<OpenIddictClientOptions>>().CurrentValue;

        var registration = Assert.Single(clientOptions.Registrations);
        Assert.Equal(new Uri("https://idp1.example.com/"), registration.Issuer);
    }

    [Fact]
    public void Configure_WebProviderRegistration_IsConfiguredByTheWebIntegration()
    {
        using var services = BuildClientServices(new OpenIddictClientRegistration
        {
            ProviderType = OpenIddictClientWebIntegrationConstants.ProviderTypes.GitHub,
            ProviderSettings = new OpenIddictClientWebIntegrationSettings.GitHub(),
            ProviderName = "GitHubOidc",
            ProviderDisplayName = "GitHub",
            ClientId = "client",
            ClientSecret = "secret",
            RedirectUri = new Uri("signin-github-oidc", UriKind.Relative),
        });

        var authenticationOptions = services.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        var clientOptions = services.GetRequiredService<IOptionsMonitor<OpenIddictClientOptions>>().CurrentValue;

        var registration = Assert.Single(clientOptions.Registrations);
        Assert.Equal(new Uri("https://github.com/login/oauth"), registration.Issuer);
        Assert.Contains(authenticationOptions.Schemes, scheme => scheme.Name == "GitHubOidc" && scheme.DisplayName == "GitHub");
    }

    [Fact]
    public void Configure_BuiltInProviders_ResolvesTheAuthenticationSchemesWithoutACycle()
    {
        var site = new Mock<ISite>();
        site.Setup(site => site.GetOrCreate<OpenIdClientSettings>()).Returns(new OpenIdClientSettings());

        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site.Object);

        var documentManager = new Mock<IDocumentManager<OpenIdClientRegistrationsDocument>>();
        documentManager.Setup(manager => manager.GetOrCreateImmutableAsync(It.IsAny<Func<Task<OpenIdClientRegistrationsDocument>>>()))
            .ReturnsAsync(new OpenIdClientRegistrationsDocument());

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDataProtection();
        services.AddSingleton(new ShellSettings { Name = ShellSettings.DefaultShellName });
        services.AddSingleton(siteService.Object);
        services.AddSingleton(documentManager.Object);

        new ClientStartup().ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();

        // The registration service validates names against the authentication schemes, which are built from the
        // registrations, so resolving the schemes must not resolve the scheme provider again.
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        Assert.NotNull(schemeProvider);
        Assert.DoesNotContain(serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value.Schemes,
            scheme => scheme.Name == OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task CreateAsync_SettingsWithSecrets_ReadsTheClientSecretAndTheSigningKeyFromSecrets()
    {
        using var rsa = RSA.Create(2048);
        var secretManager = new Mock<ISecretManager>();
        secretManager.Setup(manager => manager.GetSecretAsync<TextSecret>("ClientSecret"))
            .ReturnsAsync(new TextSecret { Text = "secret-from-store" });
        secretManager.Setup(manager => manager.GetSecretAsync<RsaKeySecret>("SigningKey"))
            .ReturnsAsync(new RsaKeySecret
            {
                PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
                IncludesPrivateKey = true,
            });

        var factory = CreateFactory(services => services.AddSingleton(secretManager.Object));

        var settings = new OpenIdClientSettings
        {
            DisplayName = "Partner IdP",
            Authority = new Uri("https://idp.example.com/"),
            ClientId = "client",
            ClientSecretSecretName = "ClientSecret",
            SigningKeySecretName = "SigningKey",
            CallbackPath = "/signin-partner",
            Scopes = ["email", "phone"],
            ResponseType = OpenIdConstants.Client.ResponseTypes.Code,
            ResponseMode = "form_post",
        };

        var registration = await factory.CreateAsync(settings, "Partner", "signin-oidc", "signout-callback-oidc");

        Assert.Equal("Partner", registration.ProviderName);
        Assert.Equal("Partner IdP", registration.ProviderDisplayName);
        Assert.Equal("secret-from-store", registration.ClientSecret);
        Assert.Single(registration.SigningCredentials);
        Assert.Equal(new Uri("signin-partner", UriKind.Relative), registration.RedirectUri);
        Assert.Equal(new Uri("signout-callback-oidc", UriKind.Relative), registration.PostLogoutRedirectUri);
        Assert.Equal(["email", "phone"], registration.Scopes.Order());
        Assert.Equal([OpenIdConstants.Client.ResponseTypes.Code], registration.ResponseTypes);
        Assert.Same(settings, registration.Properties[OpenIdConstants.Client.SettingsProperty]);
    }

    [Fact]
    public async Task CreateAsync_ProtectedClientSecret_UnprotectsIt()
    {
        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        var factory = CreateFactory(services => services.AddSingleton<IDataProtectionProvider>(dataProtectionProvider));

        var settings = new OpenIdClientSettings
        {
            Authority = new Uri("https://idp.example.com/"),
            ClientId = "client",
            ClientSecret = dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration)).Protect("protected-secret"),
        };

        var registration = await factory.CreateAsync(settings, "Partner", "signin-oidc", "signout-callback-oidc");

        Assert.Equal("protected-secret", registration.ClientSecret);
        Assert.Empty(registration.SigningCredentials);
    }

    [Theory]
    [InlineData("Partner", true)]
    [InlineData("Partner_2-b", true)]
    [InlineData("2Partner", false)]
    [InlineData("Part ner", false)]
    [InlineData("OpenIdConnect", false)]
    [InlineData("Existing", false)]
    [InlineData("Google", false)]
    public async Task ValidateRegistrationAsync_Name_ValidatesTheNameIsAUniqueScheme(string name, bool valid)
    {
        var service = CreateRegistrationService(existing: new OpenIdClientRegistration { Name = "Existing" }, schemes: ["Google"]);

        var results = await service.ValidateRegistrationAsync(CreateValidSettings(name));

        Assert.Equal(valid, !results.Any(result => result != ValidationResult.Success && result.MemberNames.Contains(nameof(OpenIdClientRegistration.Name))));
    }

    [Fact]
    public async Task ValidateRegistrationAsync_EditedRegistration_AcceptsItsOwnName()
    {
        var service = CreateRegistrationService(existing: new OpenIdClientRegistration { Name = "Existing" }, schemes: ["Existing"]);

        var results = await service.ValidateRegistrationAsync(CreateValidSettings("Existing"), originalName: "Existing");

        Assert.All(results, result => Assert.Equal(ValidationResult.Success, result));
    }

    [Fact]
    public async Task OpenIdClientRegistrationsStep_Registrations_ProtectsTheSecretAndKeepsTheExistingOne()
    {
        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        var protector = dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration));
        var saved = new List<OpenIdClientRegistration>();

        var registrationService = new Mock<IOpenIdClientRegistrationService>();
        registrationService.Setup(service => service.GetRegistrationAsync("Existing"))
            .ReturnsAsync(new OpenIdClientRegistration { Name = "Existing", ClientSecret = protector.Protect("existing-secret") });
        registrationService.Setup(service => service.ValidateRegistrationAsync(It.IsAny<OpenIdClientRegistration>(), It.IsAny<string>()))
            .ReturnsAsync(ImmutableArray<ValidationResult>.Empty);
        registrationService.Setup(service => service.UpdateRegistrationAsync(It.IsAny<OpenIdClientRegistration>()))
            .Callback<OpenIdClientRegistration>(saved.Add)
            .Returns(Task.CompletedTask);

        var shellReleaseManager = new Mock<IShellReleaseManager>();

        var step = new OpenIdClientRegistrationsStep(
            registrationService.Object,
            dataProtectionProvider,
            shellReleaseManager.Object,
            CreateLocalizer<OpenIdClientRegistrationsStep>());

        var context = new RecipeExecutionContext
        {
            Name = OpenIdClientRegistrationsStep.RecipeStepName,
            Step = JsonNode.Parse(
                """
                {
                    "name": "OpenIdClientRegistrations",
                    "Registrations": [
                        {
                            "Name": "Partner",
                            "DisplayName": "Partner IdP",
                            "Authority": "https://idp.example.com/",
                            "ClientId": "client",
                            "ClientSecret": "new-secret",
                            "Scopes": "email, phone",
                            "ResponseType": "code",
                            "ResponseMode": "form_post"
                        },
                        {
                            "Name": "Existing",
                            "DisplayName": "Existing IdP",
                            "Authority": "https://existing.example.com/",
                            "ClientId": "client",
                            "ResponseType": "code",
                            "ResponseMode": "form_post"
                        }
                    ]
                }
                """).AsObject(),
        };

        await step.ExecuteAsync(context);

        Assert.Empty(context.Errors);
        Assert.Equal(2, saved.Count);

        var partner = saved[0];
        Assert.Equal("Partner", partner.Name);
        Assert.Equal("new-secret", protector.Unprotect(partner.ClientSecret));
        Assert.Equal(["email", "phone"], partner.Scopes);

        var existing = saved[1];
        Assert.Equal("existing-secret", protector.Unprotect(existing.ClientSecret));
        Assert.Equal(new Uri("https://existing.example.com/"), existing.Authority);

        shellReleaseManager.Verify(manager => manager.RequestRelease(), Times.Once());
    }

    private static ServiceProvider BuildClientServices(params OpenIddictClientRegistration[] registrations)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton(new ShellSettings { Name = ShellSettings.DefaultShellName });

        new ClientStartup().ConfigureServices(services);

        // The registrations are supplied by a test provider instead of the site settings and the stored documents.
        services.RemoveAll<IOpenIdClientRegistrationProvider>();
        services.AddSingleton<IOpenIdClientRegistrationProvider>(new TestRegistrationProvider(registrations));

        return services.BuildServiceProvider();
    }

    private static OpenIddictClientRegistration CreateRegistration(string providerName, string issuer, string redirectUri, string postLogoutRedirectUri)
        => new()
        {
            ProviderName = providerName,
            ProviderDisplayName = $"{providerName} IdP",
            Issuer = new Uri(issuer),
            ClientId = "client",
            RedirectUri = new Uri(redirectUri, UriKind.Relative),
            PostLogoutRedirectUri = new Uri(postLogoutRedirectUri, UriKind.Relative),
        };

    private static OpenIdClientRegistrationFactory CreateFactory(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        configure(services);

        var serviceProvider = services.BuildServiceProvider();

        return new OpenIdClientRegistrationFactory(
            serviceProvider,
            serviceProvider.GetRequiredService<IDataProtectionProvider>(),
            NullLogger<OpenIdClientRegistrationFactory>.Instance);
    }

    private static OpenIdClientRegistrationService CreateRegistrationService(OpenIdClientRegistration existing, string[] schemes)
    {
        var document = new OpenIdClientRegistrationsDocument();
        document.Registrations[existing.Name] = existing;

        var documentManager = new Mock<IDocumentManager<OpenIdClientRegistrationsDocument>>();
        documentManager.Setup(manager => manager.GetOrCreateImmutableAsync(It.IsAny<Func<Task<OpenIdClientRegistrationsDocument>>>()))
            .ReturnsAsync(document);

        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider.Setup(provider => provider.GetAllSchemesAsync())
            .ReturnsAsync(schemes.Select(name => new AuthenticationScheme(name, name, typeof(IAuthenticationHandler))).ToArray());

        var clientService = new Mock<IOpenIdClientService>();
        clientService.Setup(service => service.ValidateSettingsAsync(It.IsAny<OpenIdClientSettings>()))
            .ReturnsAsync(ImmutableArray<ValidationResult>.Empty);

        var services = new ServiceCollection()
            .AddSingleton(schemeProvider.Object)
            .BuildServiceProvider();

        return new OpenIdClientRegistrationService(
            documentManager.Object,
            clientService.Object,
            services,
            CreateLocalizer<OpenIdClientRegistrationService>());
    }

    private static OpenIdClientRegistration CreateValidSettings(string name)
        => new()
        {
            Name = name,
            DisplayName = "IdP",
            Authority = new Uri("https://idp.example.com/"),
            ClientId = "client",
            ResponseType = OpenIdConstants.Client.ResponseTypes.Code,
            ResponseMode = "form_post",
        };

    private static StringLocalizer<T> CreateLocalizer<T>()
        => new StringLocalizer<T>(new ResourceManagerStringLocalizerFactory(
            Options.Create(new LocalizationOptions()),
            NullLoggerFactory.Instance));

    private sealed class TestRegistrationProvider : IOpenIdClientRegistrationProvider
    {
        private readonly OpenIddictClientRegistration[] _registrations;

        public TestRegistrationProvider(OpenIddictClientRegistration[] registrations)
        {
            _registrations = registrations;
        }

        public Task<IEnumerable<OpenIddictClientRegistration>> GetRegistrationsAsync()
            => Task.FromResult<IEnumerable<OpenIddictClientRegistration>>(_registrations);
    }
}
