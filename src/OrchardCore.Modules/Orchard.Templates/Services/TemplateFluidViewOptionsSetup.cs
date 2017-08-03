using System;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Fluid;

namespace Orchard.Templates.Services
{
    public class TemplateFluidViewOptionsSetup : IConfigureOptions<FluidViewOptions>
    {
        private readonly ITemplateFileProvider _templateFileProvider;

        public TemplateFluidViewOptionsSetup(ITemplateFileProvider templateFileProvider)
        {
            _templateFileProvider = templateFileProvider ?? throw new ArgumentNullException(nameof(templateFileProvider));
        }

        public void Configure(FluidViewOptions options)
        {
            options.FileProviders.Insert(0, _templateFileProvider);
        }
    }
}