using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;

namespace OrchardCore.DisplayManagement.Liquid;

public sealed class LiquidShapeTemplateOptionsSetup : IConfigureOptions<ShapeTemplateOptions>
{
    private readonly LiquidViewOptions _liquidViewOptions;

    public LiquidShapeTemplateOptionsSetup(IOptions<LiquidViewOptions> liquidViewOptions)
    {
        _liquidViewOptions = liquidViewOptions.Value;
    }

    public void Configure(ShapeTemplateOptions options)
    {
        options.FileProviders.Insert(0, _liquidViewOptions.FileProvider);
    }
}
