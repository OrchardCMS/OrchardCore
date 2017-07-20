using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.Settings;

namespace Orchard.Templates.Services
{
    public class TemplateShapeTemplateOptions : ConfigureOptions<ShapeTemplateOptions>
    {
        public TemplateShapeTemplateOptions(TemplatesManager templatesManager, ISiteService siteService)
            : base(options => ConfigureShapeTemplate(options, templatesManager, siteService))
        {
        }

        private static void ConfigureShapeTemplate(ShapeTemplateOptions options,
            TemplatesManager templatesManager, ISiteService siteService)
        {
            options.FileProviders.Insert(0, new TemplateFileProvider(templatesManager, siteService));
        }
    }
}