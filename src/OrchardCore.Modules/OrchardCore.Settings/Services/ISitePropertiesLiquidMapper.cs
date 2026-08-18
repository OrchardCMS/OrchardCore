using Fluid.Values;

namespace OrchardCore.Settings.Services;

public interface ISitePropertiesLiquidMapper
{
    Task<FluidValue> MapAsync(ISite site);
}
