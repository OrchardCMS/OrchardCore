using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Filters;
using OrchardCore.Facebook.Recipes;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Facebook
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.UseMiddleware<ScriptsMiddleware>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<IFacebookService, FacebookService>();
            services.AddScoped<IDisplayDriver<ISite>, FacebookSettingsDisplayDriver>();
            services.AddRecipeExecutionStep<FacebookSettingsStep>();

            services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
            services.AddTransient<IConfigureOptions<FacebookSettings>, FacebookSettingsConfiguration>();

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(FBInitFilter));
            });
        }
    }
}
