using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidRazorViewOptionsSetup : IConfigureOptions<RazorViewEngineOptions>
    {
        private readonly IFluidRazorViewFileProvider _fileProvider;

        public FluidRazorViewOptionsSetup(IFluidRazorViewFileProvider fileProvider)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        }

        public void Configure(RazorViewEngineOptions options)
        {
            options.FileProviders.Insert(0, _fileProvider);
        }
    }
}