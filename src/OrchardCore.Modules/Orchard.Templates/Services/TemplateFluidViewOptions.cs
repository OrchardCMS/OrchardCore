using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Fluid;
using Orchard.DisplayManagement.Theming;

namespace Orchard.Templates.Services
{
    public class TemplateFluidViewOptions : ConfigureOptions<FluidViewOptions>
    {
        public TemplateFluidViewOptions(TemplatesManager templatesManager, IThemeManager themanager)
            : base(options => ConfigureFluid(options, templatesManager, themanager))
        {
        }

        private static void ConfigureFluid(FluidViewOptions options,
            TemplatesManager templatesManager, IThemeManager themanager)
        {
            options.FileProviders.Insert(0, new TemplateFileProvider(templatesManager, themanager));
        }
    }
}