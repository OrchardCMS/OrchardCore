using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Mvc.RazorPages
{
    public static class ModularPageMvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddModularRazorPages(this IMvcCoreBuilder builder, IServiceProvider services)
        {
            builder.AddRazorPages(options =>
            {
                options.RootDirectory = "/";
                var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
                options.Conventions.Add(new DefaultModularPageRouteModelConvention(httpContextAccessor));
            });

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPageApplicationModelProvider, ModularPageApplicationModelProvider>());

            return builder;
        }
    }
}
