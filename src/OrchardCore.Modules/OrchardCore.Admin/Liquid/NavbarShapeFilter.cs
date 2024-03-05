using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Liquid;

namespace OrchardCore.Admin.Liquid;

public class NavbarShapeFilter : ILiquidFilter
{
    private readonly IDisplayManager<Navbar> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public NavbarShapeFilter(
        IDisplayManager<Navbar> displayManager,
        IUpdateModelAccessor updateModelAccessor)
    {
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var shape = await _displayManager.BuildDisplayAsync(_updateModelAccessor.ModelUpdater);

        return FluidValue.Create(shape, context.Options);
    }
}
