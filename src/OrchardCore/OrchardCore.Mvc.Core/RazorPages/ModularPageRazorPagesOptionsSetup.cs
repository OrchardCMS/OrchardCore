using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRazorPagesOptionsSetup : IConfigureOptions<RazorPagesOptions>
    {
        private readonly IApplicationContext _applicationContext;

        public ModularPageRazorPagesOptionsSetup(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void Configure(RazorPagesOptions options)
        {
            options.Conventions.AddFolderRouteModelConvention("/", model => model.Selectors.Clear());

            options.Conventions.AddAreaFolderRouteModelConvention(_applicationContext.Application.Name, "/",
                model =>
            {
                foreach (var selector in model.Selectors.ToArray())
                {
                    var template = selector.AttributeRouteModel.Template;

                    if (template.StartsWith(_applicationContext.Application.Name))
                    {
                        template = template.Substring(_applicationContext.Application.Name.Length).TrimStart('/');
                    }

                    selector.AttributeRouteModel.Template = template;
                }
            });
        }
    }
}