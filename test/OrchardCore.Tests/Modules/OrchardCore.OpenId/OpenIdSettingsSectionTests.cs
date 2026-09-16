using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdSettingsSectionTests
{
    [Fact]
    public async Task ClientSecret_WriteOnlyProtectedRetainedOnRetryAndExplicitlyCleared()
    {
        var settings = new OpenIdClientSettings { ClientId = "existing", ResponseType = "code", ResponseMode = "query", Authority = new Uri("https://example.test") };
        var service = new Mock<IOpenIdClientService>();
        service.Setup(service => service.GetSettingsAsync()).ReturnsAsync(() => settings);
        service.Setup(service => service.ValidateSettingsAsync(It.IsAny<OpenIdClientSettings>())).ReturnsAsync(ImmutableArray<ValidationResult>.Empty);
        service.Setup(service => service.UpdateSettingsAsync(It.IsAny<OpenIdClientSettings>()))
            .Callback<OpenIdClientSettings>(value => settings = value).Returns(Task.CompletedTask);
        var release = new Mock<IShellReleaseManager>();
        var protection = new EphemeralDataProtectionProvider();
        var services = new ServiceCollection().AddSingleton(service.Object).AddSingleton(release.Object).AddSingleton<IDataProtectionProvider>(protection);
        new OpenIdClientManagementStartup().ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var section = provider.GetRequiredService<ISiteSettingsSectionProvider>();
        var input = new JsonObject { ["clientSecret"] = "private-test-secret", ["parameters"] = new JsonArray(new JsonObject { ["name"] = "custom", ["value"] = "private-parameter" }) };
        var result = await section.UpdateAsync(input);
        Assert.True(result.Changed);
        Assert.True(result.ReloadRequested);
        Assert.DoesNotContain("private-", result.Section.Values.ToJsonString());
        Assert.True(result.Section.Values["hasClientSecret"].GetValue<bool>());
        Assert.Contains("clientSecret", result.Section.RedactedProperties);
        Assert.Contains("parameters", result.Section.RedactedProperties);
        Assert.NotEqual("private-test-secret", settings.ClientSecret);
        Assert.Equal("private-test-secret", protection.CreateProtector("OpenIdClientConfiguration").Unprotect(settings.ClientSecret));
        Assert.Equal("existing", settings.ClientId);
        var encrypted = settings.ClientSecret;
        Assert.False((await section.UpdateAsync(input)).Changed);
        Assert.Equal(encrypted, settings.ClientSecret);
        release.Verify(release => release.RequestRelease(), Times.Once);
        var cleared = await section.UpdateAsync(new JsonObject { ["clientSecret"] = null, ["parameters"] = null });
        Assert.True(cleared.Changed);
        Assert.Null(settings.ClientSecret);
        Assert.Empty(settings.Parameters);
        Assert.False(cleared.Section.Values["hasClientSecret"].GetValue<bool>());
    }

    [Fact]
    public async Task Server_InvalidSchemaAndSharedValidationDoNotPersistOrReload()
    {
        var settings = new OpenIdServerSettings { AllowClientCredentialsFlow = true, TokenEndpointPath = "/connect/token" };
        var service = new Mock<IOpenIdServerService>();
        service.Setup(service => service.GetSettingsAsync()).ReturnsAsync(settings);
        service.Setup(service => service.ValidateSettingsAsync(It.IsAny<OpenIdServerSettings>()))
            .ReturnsAsync([new ValidationResult("The token endpoint is required.", ["TokenEndpointPath"])]);
        var release = new Mock<IShellReleaseManager>();
        var services = new ServiceCollection().AddSingleton(service.Object).AddSingleton(release.Object);
        new OpenIdServerManagementStartup().ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var section = provider.GetRequiredService<ISiteSettingsSectionProvider>();
        Assert.True(section.Descriptor.RequiresHttps);
        Assert.Equal(OpenIdPermissions.ManageServerSettings, section.UpdatePermission);
        foreach (var input in new JsonObject[]
        {
            new() { ["allowClientCredentialsFlow"] = null },
            new() { ["unknown"] = true },
            new() { ["authority"] = "relative" },
            new() { ["accessTokenFormat"] = 900 },
            new() { ["tokenEndpointPath"] = "relative" },
        })
        {
            Assert.NotEmpty((await section.UpdateAsync(input)).Errors);
        }
        var invalid = await section.UpdateAsync(new JsonObject { ["tokenEndpointPath"] = null });
        Assert.Contains("tokenEndpointPath", invalid.Errors.Keys);
        Assert.Equal("/connect/token", settings.TokenEndpointPath.Value);
        service.Verify(service => service.UpdateSettingsAsync(It.IsAny<OpenIdServerSettings>()), Times.Never);
        release.Verify(release => release.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task Validation_UsesExistingServiceAndRetainsUnchangedSettings()
    {
        var settings = new OpenIdValidationSettings { Tenant = "Default" };
        var service = new Mock<IOpenIdValidationService>();
        service.Setup(service => service.GetSettingsAsync()).ReturnsAsync(() => settings);
        service.Setup(service => service.ValidateSettingsAsync(It.IsAny<OpenIdValidationSettings>())).ReturnsAsync(ImmutableArray<ValidationResult>.Empty);
        service.Setup(service => service.UpdateSettingsAsync(It.IsAny<OpenIdValidationSettings>()))
            .Callback<OpenIdValidationSettings>(value => settings = value).Returns(Task.CompletedTask);
        var release = new Mock<IShellReleaseManager>();
        var services = new ServiceCollection().AddSingleton(service.Object).AddSingleton(release.Object);
        new OpenIdValidationManagementStartup().ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var section = provider.GetRequiredService<ISiteSettingsSectionProvider>();
        Assert.False((await section.UpdateAsync(new JsonObject { ["tenant"] = "Default" })).Changed);
        var update = await section.UpdateAsync(new JsonObject { ["tenant"] = null, ["authority"] = "https://example.test/", ["audience"] = "resource" });
        Assert.True(update.Changed);
        Assert.Null(settings.Tenant);
        Assert.Equal(new Uri("https://example.test/"), settings.Authority);
        service.Verify(service => service.ValidateSettingsAsync(It.IsAny<OpenIdValidationSettings>()), Times.Exactly(2));
        release.Verify(release => release.RequestRelease(), Times.Once);
    }
}
