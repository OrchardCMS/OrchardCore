using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
namespace OrchardCore.SuperEdit
{
    public class Startup : StartupBase
    {

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            //services.AddNodeServices();
        }
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {

            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
            var app = serviceProvider.GetRequiredService<IApplicationContext>();
            var module = app.Application.Modules.Where(x => x.Name == "OrchardCore.SuperEdit").FirstOrDefault();
            var asset = module.Assets.Where(x => x.ModuleAssetPath.EndsWith("webpack.config.js")).FirstOrDefault();

            if (env.IsDevelopment())
            {
                builder.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ConfigFile = asset.ProjectAssetPath,
                    ProjectPath = Path.GetDirectoryName(asset.ProjectAssetPath),
                    //ConfigFile = Path.Combine(env.ContentRootPath, @"~/OrchardCore.SuperEdit/webpack.config.js"),
                    //ProjectPath = Path.Combine(env.ContentRootPath, @"~/OrchardCore.SuperEdit/Assets"),
                });
            }
            routes.MapAreaRoute(
                name: "supperedit",
                areaName: "OrchardCore.SuperEdit",
                template: "supper/List/{unseid?}",
                defaults: new { controller = "Home", action = "List" }
            );

        }
    }
}
