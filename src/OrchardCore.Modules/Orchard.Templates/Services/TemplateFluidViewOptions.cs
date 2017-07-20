using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Fluid;
using Orchard.Settings;

namespace Orchard.Templates.Services
{
    public class TemplateFluidViewOptions : ConfigureOptions<FluidViewOptions>
    {
        public TemplateFluidViewOptions(TemplatesManager templatesManager, ISiteService siteService)
            : base(options => ConfigureFluid(options, templatesManager, siteService))
        {
        }

        private static void ConfigureFluid(FluidViewOptions options,
            TemplatesManager templatesManager, ISiteService siteService)
        {
            options.FileProviders.Insert(0, new TemplateFileProvider(templatesManager, siteService));
        }
    }
}