using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Resources;

public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private readonly ResourceOptions _resourceOptions;
    private readonly IHostEnvironment _env;
    private readonly PathString _pathBase;

    public ResourceManagementOptionsConfiguration(
        IOptions<ResourceOptions> resourceOptions,
        IHostEnvironment env,
        IHttpContextAccessor httpContextAccessor)
    {
        _resourceOptions = resourceOptions.Value;
        _env = env;
        _pathBase = httpContextAccessor.HttpContext.Request.PathBase;
    }

    ResourceManifest BuildManifest() => ResourceManifestGenerator.Build(_pathBase.Value);

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(BuildManifest());

        switch (_resourceOptions.ResourceDebugMode)
        {
            case ResourceDebugMode.Enabled:
                options.DebugMode = true;
                break;

            case ResourceDebugMode.Disabled:
                options.DebugMode = false;
                break;

            case ResourceDebugMode.FromConfiguration:
                options.DebugMode = !_env.IsProduction();
                break;
        }

        options.UseCdn = _resourceOptions.UseCdn;
        options.CdnBaseUrl = _resourceOptions.CdnBaseUrl;
        options.AppendVersion = _resourceOptions.AppendVersion;
        options.ContentBasePath = _pathBase.Value;
    }
}
