using Fluid;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidShapeTemplateOptionsSetup : IConfigureOptions<ShapeTemplateOptions>
    {
        private readonly TemplateOptions _templateOptions;

        public LiquidShapeTemplateOptionsSetup(TemplateOptions templateOptions)
        {
            _templateOptions = templateOptions;
        }

        public void Configure(ShapeTemplateOptions options)
        {
            options.FileProviders.Insert(0, _templateOptions.FileProvider);
        }
    }
}
