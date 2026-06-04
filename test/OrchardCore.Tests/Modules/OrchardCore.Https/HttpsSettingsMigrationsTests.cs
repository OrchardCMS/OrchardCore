using System.Text.Json.Nodes;
using Moq;
using OrchardCore.Https.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Https;

public class HttpsSettingsMigrationsTests
{
    private const string LegacyEnableStrictTransportSecurityKey = "EnableStrictTransportSecurity";

    [Fact]
    public async Task CreateAsyncMigratesLegacyHstsSetting()
    {
        var site = new SiteSettings();
        site.Properties[nameof(HttpsSettings)] = new JsonObject
        {
            [LegacyEnableStrictTransportSecurityKey] = true,
        };

        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        siteService.Setup(service => service.UpdateSiteSettingsAsync(site)).Returns(Task.CompletedTask);

        var version = await InvokeCreateAsync(siteService.Object);

        Assert.Equal(1, version);
        Assert.Equal(
            HttpStrictTransportSecurityMode.Enabled.ToString(),
            site.Properties[nameof(HttpsSettings)]?[nameof(HttpsSettings.StrictTransportSecurityMode)]?.GetValue<string>());
        Assert.Null(site.Properties[nameof(HttpsSettings)]?[LegacyEnableStrictTransportSecurityKey]);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Once);
    }

    [Fact]
    public async Task CreateAsyncRemovesLegacySettingWithoutOverwritingMigratedValue()
    {
        var site = new SiteSettings();
        site.Properties[nameof(HttpsSettings)] = new JsonObject
        {
            [nameof(HttpsSettings.StrictTransportSecurityMode)] = HttpStrictTransportSecurityMode.Disabled.ToString(),
            [LegacyEnableStrictTransportSecurityKey] = true,
        };

        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        siteService.Setup(service => service.UpdateSiteSettingsAsync(site)).Returns(Task.CompletedTask);

        var version = await InvokeCreateAsync(siteService.Object);

        Assert.Equal(1, version);
        Assert.Equal(
            HttpStrictTransportSecurityMode.Disabled.ToString(),
            site.Properties[nameof(HttpsSettings)]?[nameof(HttpsSettings.StrictTransportSecurityMode)]?.GetValue<string>());
        Assert.Null(site.Properties[nameof(HttpsSettings)]?[LegacyEnableStrictTransportSecurityKey]);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Once);
    }

    private static async Task<int> InvokeCreateAsync(ISiteService siteService)
    {
        var migrationType = typeof(HttpsSettings).Assembly.GetType("OrchardCore.Https.Migrations.HttpsSettingsMigrations");
        if (migrationType is null)
        {
            throw new InvalidOperationException("Unable to locate the HTTPS settings migration type.");
        }

        var migration = Activator.CreateInstance(migrationType, siteService);
        if (migration is null)
        {
            throw new InvalidOperationException("Unable to create the HTTPS settings migration instance.");
        }

        var method = migrationType.GetMethod("CreateAsync");
        if (method is null)
        {
            throw new InvalidOperationException("Unable to locate the HTTPS settings migration CreateAsync method.");
        }

        var result = method.Invoke(migration, null) as Task<int>;
        if (result is null)
        {
            throw new InvalidOperationException("Unable to invoke the HTTPS settings migration CreateAsync method.");
        }

        return await result;
    }
}
