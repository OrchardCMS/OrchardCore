using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using OrchardCore.OpenId.Drivers;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;
using OrchardCore.Tests.OptionsMonitoring;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdClientSettingsDisplayDriverTests
{
    [Fact]
    public async Task UpdateAsync_ValidModel_RequestsOpenIdConnectOptionsUpdates()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, isValid: true);

        Assert.Equal(
        [
            (typeof(OpenIdConnectOptions), OpenIdConnectDefaults.AuthenticationScheme),
            (typeof(AuthenticationOptions), ""),
        ], notifier.Requests);
    }

    [Fact]
    public async Task UpdateAsync_InvalidModel_RequestsNoUpdate()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, isValid: false);

        Assert.Empty(notifier.Requests);
    }

    private static async Task UpdateAsync(RecordingOptionsUpdateNotifier notifier, bool isValid)
    {
        var clientService = new Mock<IOpenIdClientService>();
        clientService
            .Setup(service => service.ValidateSettingsAsync(It.IsAny<OpenIdClientSettings>()))
            .ReturnsAsync(ImmutableArray<ValidationResult>.Empty);

        var driver = new OpenIdClientSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            new EphemeralDataProtectionProvider(),
            clientService.Object,
            SettingsDriverTestHelper.HttpContextAccessor(),
            Mock.Of<IStringLocalizer<OpenIdClientSettingsDisplayDriver>>());

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new OpenIdClientSettingsViewModel
            {
                Authority = "https://idp.example.com/",
                ClientId = "client",
                ClientSecret = "secret",
                ResponseMode = "form_post",
                UseCodeFlow = true,
            },
            "OrchardCore.OpenId.Client",
            isValid);

        await driver.UpdateAsync(new SiteSettings(), new OpenIdClientSettings(), context);
    }
}
