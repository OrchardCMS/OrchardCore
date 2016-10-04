using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Razor;
using Orchard.DisplayManagement.Theming;

namespace Orchard.DisplayManagement
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrchardTheming(this IServiceCollection services)
        {
            services.AddTransient<IMvcRazorHost, ShapeRazorHost>();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(new ThemingFileProvider());
            });

            return services;
        }
    }
}
