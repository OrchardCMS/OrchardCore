using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class MediaSettingsSectionTests
{
    [Theory]
    [InlineData("{\"maxFileSize\":0}")]
    [InlineData("{\"maxFileSize\":30000001}")]
    [InlineData("{\"allowedFileExtensions\":[\".exe\"]}")]
    [InlineData("{\"maxFileSize\":10,\"effectiveMaxFileSize\":10}")]
    [InlineData("{\"allowedFileExtensions\":[null]}")]
    public async Task InvalidUploadUpdatesNeverPartiallyPersist(string json)
    {
        var fixture = Create();
        var provider = new MediaUploadSettingsSectionProvider(fixture.Service.Object, fixture.Release.Object, Configuration());
        Assert.NotEmpty((await provider.UpdateAsync(JsonNode.Parse(json).AsObject())).Errors);
        Assert.Null(fixture.Site.As<MediaUploadSettings>().MaxFileSize);
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(release => release.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task UploadOverrideRetryAndNullResetPreserveHostOwnership()
    {
        var fixture = Create();
        var provider = new MediaUploadSettingsSectionProvider(fixture.Service.Object, fixture.Release.Object, Configuration());
        var patch = JsonNode.Parse("{\"maxFileSize\":100,\"allowedFileExtensions\":[\".PNG\",\".svg\"]}").AsObject();
        var changed = await provider.UpdateAsync(patch);
        Assert.True(changed.Changed);
        Assert.Equal(100, changed.Section.Values["effectiveMaxFileSize"].GetValue<long>());
        Assert.Equal(30000000, changed.Section.Values["hostMaxFileSize"].GetValue<long>());
        Assert.Equal([".png"], changed.Section.Values["effectiveAllowedFileExtensions"].AsArray().Select(value => value.GetValue<string>()));
        Assert.Equal([".svg"], changed.Section.Values["effectiveRestrictedFileExtensions"].AsArray().Select(value => value.GetValue<string>()));
        Assert.Equal(changed.Section.Values.ToJsonString(), (await provider.GetAsync()).Values.ToJsonString());
        Assert.False((await provider.UpdateAsync(patch)).Changed);
        Assert.False((await provider.UpdateAsync([])).Changed);
        var reset = await provider.UpdateAsync(new JsonObject { ["maxFileSize"] = null, ["allowedFileExtensions"] = null });
        Assert.True(reset.Changed);
        Assert.Equal(30000000, reset.Section.Values["effectiveMaxFileSize"].GetValue<long>());
        Assert.Null(fixture.Site.As<MediaUploadSettings>().AllowedFileExtensions);
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(fixture.Site), Times.Exactly(2));
        fixture.Release.Verify(release => release.RequestRelease(), Times.Exactly(2));
    }

    [Theory]
    [InlineData("{\"authenticationScheme\":\"0\"}")]
    [InlineData("{\"authenticationScheme\":2}")]
    [InlineData("{\"authenticationScheme\":null}")]
    [InlineData("{\"authenticationScheme\":\"Bearer\",\"secret\":\"invalid\"}")]
    public async Task MediaAuthenticationRejectsInvalidValuesBeforeMutating(string json)
    {
        var fixture = Create();
        var provider = new MediaApiSettingsSectionProvider(fixture.Service.Object, fixture.Release.Object);
        Assert.NotEmpty((await provider.UpdateAsync(JsonNode.Parse(json).AsObject())).Errors);
        Assert.Equal(MediaApiAuthenticationScheme.Cookie, fixture.Site.As<MediaApiSettings>().AuthenticationScheme);
        fixture.Service.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(release => release.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task MediaAuthenticationRequestsReloadOnlyForChanges()
    {
        var fixture = Create();
        var provider = new MediaApiSettingsSectionProvider(fixture.Service.Object, fixture.Release.Object);
        var patch = new JsonObject { ["authenticationScheme"] = "Bearer" };
        Assert.True((await provider.UpdateAsync(patch)).ReloadRequested);
        Assert.False((await provider.UpdateAsync(patch)).ReloadRequested);
        Assert.Equal(MediaApiAuthenticationScheme.Bearer, fixture.Site.As<MediaApiSettings>().AuthenticationScheme);
        fixture.Release.Verify(release => release.RequestRelease(), Times.Once);
    }

    private static IShellConfiguration Configuration()
    {
        var configuration = new ConfigurationBuilder().Build();
        var shell = new Mock<IShellConfiguration>();
        shell.Setup(value => value.GetSection(It.IsAny<string>())).Returns((string key) => configuration.GetSection(key));
        return shell.Object;
    }

    private static (SiteSettings Site, Mock<ISiteService> Service, Mock<IShellReleaseManager> Release) Create()
    {
        var site = new SiteSettings { IsReadOnly = false };
        site.Put(new MediaUploadSettings());
        site.Put(new MediaApiSettings());
        var service = new Mock<ISiteService>();
        service.Setup(value => value.LoadSiteSettingsAsync()).ReturnsAsync(site);
        service.Setup(value => value.GetSiteSettingsAsync()).ReturnsAsync(site);
        return (site, service, new Mock<IShellReleaseManager>());
    }
}
