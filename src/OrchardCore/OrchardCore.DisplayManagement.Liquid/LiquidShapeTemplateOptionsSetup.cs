using System;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Liquid.Internal;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidShapeTemplateOptionsSetup : IConfigureOptions<ShapeTemplateOptions>
    {
        private readonly ILiquidViewFileProviderAccessor _fileProviderAccessor;

        public LiquidShapeTemplateOptionsSetup(ILiquidViewFileProviderAccessor fileProviderAccessor)
        {
            _fileProviderAccessor = fileProviderAccessor ?? throw new ArgumentNullException(nameof(fileProviderAccessor));
        }

        public void Configure(ShapeTemplateOptions options)
        {
            options.FileProviders.Insert(0, _fileProviderAccessor.FileProvider);
        }
    }
}
