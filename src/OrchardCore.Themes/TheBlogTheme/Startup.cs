using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace TheBlogTheme
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RazorPagesOptions>(options =>
            {
                // The module id (project / assembly name) is already defined as a default folder path
                // options.Conventions.AddModularFolderRoute("/OrchardCore.Demo/Pages", "OrchardCore.Demo");

                // Add a custom folder path
                //options.Conventions.AddModularFolderRoute("/OrchardCore.Demo/Pages", "Demo");
                //options.Conventions.AddAreaFolderRouteModelConvention("TheBlogTheme", "/", model =>
                //{
                //    foreach (var selector in model.Selectors.ToArray())
                //    {
                //        selector.AttributeRouteModel.Template = selector.AttributeRouteModel
                //            .Template.Substring("TheBlogTheme".Length).TrimStart('/');
                //    }
                //});

                options.Conventions.AddAreaFolderRouteModelConvention("TheBlogTheme", "/", model =>
                {
                    foreach (var selector in model.Selectors.ToArray())
                    {
                        if (selector.AttributeRouteModel.Template.StartsWith("TheBlogTheme"))
                        {
                            selector.AttributeRouteModel.SuppressLinkGeneration = true;

                            var template = ("Theme/" + selector.AttributeRouteModel.Template
                                .Substring("TheBlogTheme".Length).TrimStart('/')).TrimEnd('/');

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

                // Add a custom page route
                //options.Conventions.AddAreaPageRoute("TheBlogTheme", "/Bar1", "Bar1");

                // This declaration would define an home page
                //options.Conventions.AddAreaPageRoute("TheBlogTheme", "/Bar1", "");
            });
        }
    }
}
