using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
            // Only serve pages under the "Areas" folder
            options.Conventions.AddFolderRouteModelConvention("/", model => model.Selectors.Clear());

            // Add a custom folder route so that the application's modules pages are rooted.
            options.Conventions.AddAreaFolderRouteModelConvention(_applicationContext.Application.Name, "/", model =>
            {
                foreach (var selector in model.Selectors.ToArray())
                {
                    if (selector.AttributeRouteModel.Template.StartsWith(_applicationContext.Application.Name))
                    {
                        selector.AttributeRouteModel.SuppressLinkGeneration = true;

                        // Skip the application name which is the area name of the application's module.
                        var template = (selector.AttributeRouteModel.Template.Substring(_applicationContext
                            .Application.Name.Length).TrimStart('/')).TrimEnd('/');

                        model.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel
                            {
                                Template = template
                            }
                        });
                    }
                }
            });
        }
    }
}