using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Fluid;

namespace Orchard.Templates.Services
{
    public class TemplateOptionsSetup<TOptions> : IConfigureOptions<TOptions> where TOptions : class
    {
        private readonly ITemplateFileProvider _templateFileProvider;

        public TemplateOptionsSetup(ITemplateFileProvider templateFileProvider)
        {
            _templateFileProvider = templateFileProvider ?? throw new ArgumentNullException(nameof(templateFileProvider));
        }

        public void Configure(TOptions options)
        {
            if (options is FluidViewOptions)
            {
                (options as FluidViewOptions).FileProviders.Insert(0, _templateFileProvider);
            }
            else if (options is RazorViewEngineOptions)
            {
                (options as RazorViewEngineOptions).FileProviders.Insert(0, _templateFileProvider);
            }
            else if (options is ShapeTemplateOptions)
            {
                (options as ShapeTemplateOptions).FileProviders.Insert(0, _templateFileProvider);
            }
        }
    }
}