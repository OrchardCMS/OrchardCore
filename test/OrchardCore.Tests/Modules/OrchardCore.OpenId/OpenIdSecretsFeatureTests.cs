using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OrchardCore.Data.Migration;
using OrchardCore.Data.Migration.Records;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Secrets;
using OrchardCore.OpenId.Secrets.Drivers;
using OrchardCore.OpenId.Secrets.Services;
using OrchardCore.OpenId.Secrets.ViewModels;
using OrchardCore.OpenId.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Tests.Stubs;
using OpenIdSecretsMigrations = OrchardCore.OpenId.Secrets.Migrations;
using StartupBase = OrchardCore.Modules.StartupBase;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdSecretsFeatureTests
{
    [Fact]
    public void GetFeatures_SecretsFeature_BelongsToOpenIdWithExistingDependencies()
    {
        using var services = CreateHostServices();
        var manager = services.GetRequiredService<IExtensionManager>();

        var feature = Assert.Single(manager.GetFeatures(), feature => feature.Id == "OrchardCore.OpenId.Secrets");

        Assert.Equal("OrchardCore.OpenId", feature.Extension.Id);
        Assert.Equal([OpenIdConstants.Features.Server, "OrchardCore.Secrets"], feature.Dependencies);
        Assert.DoesNotContain(manager.GetExtensions(), extension => extension.Id == "OrchardCore.OpenId.Secrets");

        var typeFeatureProvider = services.GetRequiredService<ITypeFeatureProvider>();
        Assert.Equal(feature.Id, typeFeatureProvider.GetFeatureForDependency(typeof(ServerSecretsStartup)).Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ComposeAsync_SecretsFeatureEnabled_RegistersOnlyWhenExplicitlyEnabled(bool enabled)
    {
        using var hostServices = CreateHostServices();
        var manager = hostServices.GetRequiredService<IExtensionManager>();
        var typeFeatureProvider = hostServices.GetRequiredService<ITypeFeatureProvider>();
        var descriptor = new ShellDescriptor
        {
            Features =
            [
                new ShellFeature { Id = OpenIdConstants.Features.Core },
                new ShellFeature { Id = OpenIdConstants.Features.Server },
                new ShellFeature { Id = "OrchardCore.Secrets" },
            ],
        };
        if (enabled)
        {
            descriptor.Features.Add(new ShellFeature { Id = "OrchardCore.OpenId.Secrets" });
        }

        var strategy = new CompositionStrategy(manager, typeFeatureProvider, NullLogger<CompositionStrategy>.Instance);
        var blueprint = await strategy.ComposeAsync(new ShellSettings { Name = "Test" }, descriptor);
        Assert.Equal(enabled, blueprint.Dependencies.ContainsKey(typeof(ServerSecretsStartup)));

        var services = new FeatureAwareServiceCollection(new ServiceCollection());
        foreach (var startupType in blueprint.Dependencies.Keys.Where(type => type == typeof(ServerSecretsStartup)))
        {
            services.SetCurrentFeature(Assert.Single(blueprint.Dependencies[startupType]));
            ((StartupBase)Activator.CreateInstance(startupType)).ConfigureServices(services);
        }

        if (enabled)
        {
            var registrations = Assert.Single(services.FeatureCollections);
            Assert.Equal("OrchardCore.OpenId.Secrets", registrations.Key.Id);
            Assert.Equal(3, registrations.Value.Count);
        }
        else
        {
            Assert.Empty(services.FeatureCollections);
        }

        Assert.Equal(enabled, services.Any(service =>
            service.ServiceType == typeof(IDisplayDriver<OpenIdServerSettings>)
            && service.ImplementationType == typeof(OpenIdSecretSettingsDisplayDriver)
            && service.Lifetime == ServiceLifetime.Scoped));
        Assert.Equal(enabled, services.Any(service =>
            service.ServiceType == typeof(IPostConfigureOptions<OpenIddictServerOptions>)
            && service.ImplementationType == typeof(OpenIdSecretsOptionsConfiguration)
            && service.Lifetime == ServiceLifetime.Singleton));
        Assert.Equal(enabled, services.Any(service =>
            service.ServiceType == typeof(IDataMigration)
            && service.ImplementationType == typeof(OpenIdSecretsMigrations)
            && service.Lifetime == ServiceLifetime.Scoped));
    }

    [Fact]
    public void GetSettings_ExistingJson_PreservesSecretReferences()
    {
        var site = new SiteSettings
        {
            IsReadOnly = false,
            Properties = JsonNode.Parse("""
                {
                  "OpenIdSecretSettings": {
                    "SigningKeySecretName": "ExistingSigningKey",
                    "EncryptionKeySecretName": "ExistingEncryptionKey"
                  }
                }
                """).AsObject(),
        };

        var settings = site.GetOrCreate<OpenIdSecretSettings>();

        Assert.Equal("ExistingSigningKey", settings.SigningKeySecretName);
        Assert.Equal("ExistingEncryptionKey", settings.EncryptionKeySecretName);
        site.Put(settings);
        Assert.Equal("ExistingSigningKey", site.Properties["OpenIdSecretSettings"]["SigningKeySecretName"].GetValue<string>());
        Assert.Single(site.Properties);
    }

    [Fact]
    public async Task CreateAsync_ExistingCertificates_MigratesToExistingSecretNames()
    {
        var site = new SiteSettings { IsReadOnly = false };
        site.Put(new OpenIdServerSettings
        {
            SigningCertificateStoreLocation = StoreLocation.CurrentUser,
            SigningCertificateStoreName = StoreName.My,
            SigningCertificateThumbprint = "signing-thumbprint",
            EncryptionCertificateStoreLocation = StoreLocation.LocalMachine,
            EncryptionCertificateStoreName = StoreName.My,
            EncryptionCertificateThumbprint = "encryption-thumbprint",
        });
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var secretManager = new Mock<ISecretManager>();
        var migration = new OpenIdSecretsMigrations(siteService.Object, secretManager.Object);

        Assert.Equal(1, await migration.CreateAsync());

        var settings = site.GetOrCreate<OpenIdSecretSettings>();
        Assert.Equal("OpenId.SigningCertificate", settings.SigningKeySecretName);
        Assert.Equal("OpenId.EncryptionCertificate", settings.EncryptionKeySecretName);
        secretManager.Verify(manager => manager.SaveSecretAsync(
            "OpenId.SigningCertificate",
            It.Is<X509Secret>(secret => secret.StoreLocation == StoreLocation.CurrentUser
                && secret.StoreName == StoreName.My && secret.Thumbprint == "signing-thumbprint"),
            It.IsAny<SecretSaveOptions>()), Times.Once);
        secretManager.Verify(manager => manager.SaveSecretAsync(
            "OpenId.EncryptionCertificate",
            It.Is<X509Secret>(secret => secret.StoreLocation == StoreLocation.LocalMachine
                && secret.StoreName == StoreName.My && secret.Thumbprint == "encryption-thumbprint"),
            It.IsAny<SecretSaveOptions>()), Times.Once);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Once);
    }

    [Fact]
    public async Task GetFeaturesThatNeedUpdateAsync_ExistingMigrationRecord_DoesNotRerunMigration()
    {
        using var services = CreateHostServices();
        var extensionManager = services.GetRequiredService<IExtensionManager>();
        extensionManager.GetFeatures();
        var record = new DataMigrationRecord
        {
            DataMigrations =
            [
                new()
                {
                    DataMigrationClass = "OrchardCore.OpenId.Secrets.Migrations",
                    Version = 1,
                },
            ],
        };
        var query = new Mock<IQuery<DataMigrationRecord>>();
        query.Setup(query => query.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(record);
        var rootQuery = new Mock<IQuery>();
        rootQuery.Setup(query => query.For<DataMigrationRecord>(true)).Returns(query.Object);
        var session = new Mock<YesSql.ISession>();
        session.Setup(session => session.Query()).Returns(rootQuery.Object);
        var manager = new DataMigrationManager(
            services.GetRequiredService<ITypeFeatureProvider>(),
            [new OpenIdSecretsMigrations(Mock.Of<ISiteService>(MockBehavior.Strict), Mock.Of<ISecretManager>(MockBehavior.Strict))],
            session.Object,
            Mock.Of<IStore>(),
            extensionManager,
            NullLogger<DataMigrationManager>.Instance);

        Assert.Equal("OrchardCore.OpenId.Secrets.Migrations", typeof(OpenIdSecretsMigrations).FullName);
        Assert.Empty(await manager.GetFeaturesThatNeedUpdateAsync());
    }

    [Fact]
    public void CompiledViews_SecretSettingsEditor_IsIncludedInOpenId()
    {
        var items = new RazorCompiledItemLoader().LoadItems(typeof(ServerSecretsStartup).Assembly);
        var editor = Assert.Single(items, item => item.Identifier.EndsWith("/Views/OpenIdSecretSettings.Edit.cshtml", StringComparison.Ordinal));

        Assert.Equal(typeof(OpenIdSecretSettingsViewModel), Assert.Single(editor.Type.BaseType.GenericTypeArguments));
    }

    private static ServiceProvider CreateHostServices()
    {
        var applicationContext = new ModularApplicationContext(new StubHostingEnvironment(), [new OpenIdModuleNamesProvider()]);
        return new ServiceCollection()
            .AddSingleton<IApplicationContext>(applicationContext)
            .AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>()
            .AddSingleton<IFeaturesProvider, FeaturesProvider>()
            .AddSingleton<IExtensionDependencyStrategy, ExtensionDependencyStrategy>()
            .AddSingleton<IExtensionPriorityStrategy, ExtensionPriorityStrategy>()
            .AddSingleton<ILogger<ExtensionManager>>(NullLogger<ExtensionManager>.Instance)
            .AddSingleton<IExtensionManager, ExtensionManager>()
            .BuildServiceProvider();
    }

    private sealed class OpenIdModuleNamesProvider : IModuleNamesProvider
    {
        public IEnumerable<string> GetModuleNames() => ["OrchardCore.OpenId", "OrchardCore.Secrets"];
    }
}
