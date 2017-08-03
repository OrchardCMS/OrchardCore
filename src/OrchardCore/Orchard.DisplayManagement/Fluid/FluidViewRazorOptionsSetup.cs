using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Fluid.Internal;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewRazorOptionsSetup<TOptions> : IConfigureOptions<TOptions> where TOptions : class
    {
        private readonly IFluidViewRazorFileProvider _razorFileProvider;
        private readonly IFluidViewFileProviderAccessor _fileProviderAccessor;

        public FluidViewRazorOptionsSetup(
            IFluidViewRazorFileProvider razorFileProvider,
            IFluidViewFileProviderAccessor fileProviderAccessor)
        {
            _razorFileProvider = razorFileProvider ?? throw new ArgumentNullException(nameof(razorFileProvider));
            _fileProviderAccessor = fileProviderAccessor ?? throw new ArgumentNullException(nameof(fileProviderAccessor));
        }

        public void Configure(TOptions options)
        {
            if (options is RazorViewEngineOptions)
            {
                (options as RazorViewEngineOptions).FileProviders.Insert(0, _razorFileProvider);
            }
            else if (options is ShapeTemplateOptions)
            {
                (options as ShapeTemplateOptions).FileProviders.Insert(0, _fileProviderAccessor.SharedFileProvider);
                (options as ShapeTemplateOptions).FileProviders.Insert(0, _fileProviderAccessor.ShellFileProvider);
            }
        }
    }
}