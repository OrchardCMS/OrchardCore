using Fluid.Values;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace OrchardCore.Settings.Services;

public class SitePropertiesLiquidMapper : ISitePropertiesLiquidMapper
{
    private readonly SettingsLiquidOptions _options;

    public SitePropertiesLiquidMapper(IOptions<SettingsLiquidOptions> options) =>
        _options = options.Value;

    public async Task<FluidValue> MapAsync(ISite site)
    {
        var json = new JsonObject();

        foreach (var name in _options.PermittedSiteProperties ?? [])
        {
            if (site.Properties.TryGetPropertyValue(name, out var node))
            {
                json[name] = node;
            }
        }

        return new ObjectValue(json);
    }
}
