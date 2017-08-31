using System;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Liquid.Internal;

namespace Orchard.DisplayManagement.Liquid
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