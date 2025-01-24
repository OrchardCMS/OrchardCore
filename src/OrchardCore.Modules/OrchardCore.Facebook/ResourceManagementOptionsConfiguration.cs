using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Endpoints;
using OrchardCore.Facebook.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Facebook;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private readonly ResourceManifest _manifest;
    private readonly ISiteService _siteService;

    public ResourceManagementOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;

        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("fb")
            .SetDependencies("fbsdk")
            .SetUrl("~/OrchardCore.Facebook/sdk/init.js");
    }

    public async void Configure(ResourceManagementOptions options)
    {
        var settings = await _siteService.GetSettingsAsync<FacebookSettings>();

        _manifest
            .DefineScript("fbsdk")
            .SetCultures(GetSdkEndpoints.ValidFacebookCultures)
            // v parameter is for cache busting
            .SetUrl($"~/OrchardCore.Facebook/sdk/fetch_{settings.SdkJs}?v=1");

        options.ResourceManifests.Add(_manifest);
    }
}
