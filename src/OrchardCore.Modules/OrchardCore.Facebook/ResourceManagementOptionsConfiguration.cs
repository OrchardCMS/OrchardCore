using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Endpoints;
using OrchardCore.Facebook.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Facebook;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private readonly ISiteService _siteService;

    public ResourceManagementOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async void Configure(ResourceManagementOptions options)
    {
        var settings = await _siteService.GetSettingsAsync<FacebookSettings>();

        var manifest = new ResourceManifest();

        manifest
            .DefineScript("fb")
            .SetDependencies("fbsdk")
            .SetUrl($"~/OrchardCore.Facebook/sdk/init.js?v={GetSdkEndpoints.GetInitScriptEndpoint.HashCacheBustingValues(settings)}");

        manifest
            .DefineScript("fbsdk")
            .SetCultures(GetSdkEndpoints.GetFetchScriptEndpoint.ValidFacebookCultures)
            .SetUrl($"~/OrchardCore.Facebook/sdk/sdk.js?v={GetSdkEndpoints.GetInitScriptEndpoint.HashCacheBustingValues(settings)}");

        options.ResourceManifests.Add(manifest);
    }
}
