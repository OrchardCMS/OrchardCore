using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Orchard.Settings;

namespace Orchard.Templates.Services
{
    public class TemplateRazorViewOptions : ConfigureOptions<RazorViewEngineOptions>
    {
        public TemplateRazorViewOptions(TemplatesManager templatesManager, ISiteService siteService)
            : base(options => ConfigureRazor(options, templatesManager, siteService))
        {
        }

        private static void ConfigureRazor(RazorViewEngineOptions options,
            TemplatesManager templatesManager, ISiteService siteService)
        {
            options.FileProviders.Insert(0, new TemplateFileProvider(templatesManager, siteService));
        }
    }
}