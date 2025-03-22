using Fluid;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Liquid.Endpoints.Scripts;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Liquid;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private readonly LiquidViewParser _liquidViewParser;
    private readonly IOptions<TemplateOptions> _templateOptions;
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("monaco-liquid-intellisense")
            .SetUrl(
                "~/OrchardCore.Liquid/monaco/liquid-intellisense.min.js",
                "~/OrchardCore.Liquid/monaco/liquid-intellisense.js"
            )
            .SetDependencies("monaco")
            .SetVersion("1.0.0");
    }

    public ResourceManagementOptionsConfiguration(LiquidViewParser liquidViewParser, IOptions<TemplateOptions> templateOptions)
    {
        _liquidViewParser = liquidViewParser;
        _templateOptions = templateOptions;
    }

    public void Configure(ResourceManagementOptions options)
    {
        // The site is restarted when settings change

        var hash = GetIntellisenseEndpoint.HashCacheBustingValues(_liquidViewParser, _templateOptions.Value);

        var manifest = new ResourceManifest();

        manifest
            .DefineScript("liquid-intellisense")
            .SetDependencies("monaco-liquid-intellisense")
            .SetUrl($"~/OrchardCore.Liquid/Scripts/liquid-intellisense.js?v={hash}");

        options.ResourceManifests.Add(_manifest);
        options.ResourceManifests.Add(manifest);
    }
}
