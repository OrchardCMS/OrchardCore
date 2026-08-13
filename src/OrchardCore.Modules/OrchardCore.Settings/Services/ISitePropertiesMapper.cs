using Fluid.Values;

namespace OrchardCore.Settings.Services;

public interface ISitePropertiesMapper
{
    Task<FluidValue> MapAsync(ISite site);
}