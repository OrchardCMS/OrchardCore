using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.SpaServices.Configuration;
using OrchardCore.SpaServices.Drivers;
using OrchardCore.SpaServices.Recipes;
using OrchardCore.SpaServices.Settings;

namespace OrchardCore.SpaServices
{
    public class Startup : StartupBase
    {
        //public override int Order => 10000;

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var siteService = serviceProvider.GetRequiredService<ISiteService>();
            var site = siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

            if (site.HomeRoute["area"]?.ToString() == "OrchardCore.SpaServices")
            {
                app.Use(async (context, next) =>
                {

                    if (context.Request.Path.HasValue && context.Request.Path != "/")
                    {
                        await next();
                        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
                        {
                            context.Request.Path = "/";
                            await next();
                        }
                    }
                    else
                    {
                        await next();
                    }
                });
            };
            app.UseSpaStaticFiles();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<ISite>, SpaServicesSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddTransient<IConfigureOptions<SpaStaticFilesOptions>, SpaStaticFileOptionsConfiguration>();

            services.AddRecipeExecutionStep<SpaServicesSettingsStep>();

            services.AddSpaStaticFiles(c =>
            {
                if (!Directory.Exists(c.RootPath))
                {
                    Directory.CreateDirectory(c.RootPath);
                };
            });

        }
    }
}
