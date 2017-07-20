using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Theming;

namespace Orchard.Templates.Services
{
    public class TemplateShapeTemplateOptions : ConfigureOptions<ShapeTemplateOptions>
    {
        public TemplateShapeTemplateOptions(TemplatesManager templatesManager, IThemeManager themanager)
            : base(options => ConfigureShapeTemplate(options, templatesManager, themanager))
        {
        }

        private static void ConfigureShapeTemplate(ShapeTemplateOptions options,
            TemplatesManager templatesManager, IThemeManager themanager)
        {
            options.FileProviders.Insert(0, new TemplateFileProvider(templatesManager, themanager));
        }
    }
}