using System;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Fluid.Internal;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidShapeTemplateOptionsSetup : IConfigureOptions<ShapeTemplateOptions>
    {
        private readonly IFluidViewFileProviderAccessor _fileProviderAccessor;

        public FluidShapeTemplateOptionsSetup(IFluidViewFileProviderAccessor fileProviderAccessor)
        {
            _fileProviderAccessor = fileProviderAccessor ?? throw new ArgumentNullException(nameof(fileProviderAccessor));
        }

        public void Configure(ShapeTemplateOptions options)
        {
            options.FileProviders.Insert(0, _fileProviderAccessor.FileProvider);
        }
    }
}