using Fluid.Values;
using OrchardCore.Environment.Shell.Configuration;
using System.Text.Json.Nodes;

namespace OrchardCore.Settings.Services;

public class SitePropertiesMapper : ISitePropertiesMapper
{
    private readonly IShellConfiguration _shellConfiguration;

    public SitePropertiesMapper(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public async Task<FluidValue> MapAsync(ISite site)
    {
        var json = site.Properties.Clone();

        return new ObjectValue(json);
    }
}
