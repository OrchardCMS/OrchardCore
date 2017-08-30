using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Orchard.Mvc.RazorPages
{
    public static class ModularPageMvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddModularRazorPages(this IMvcCoreBuilder builder)
        {
            builder.AddRazorPages(options =>
            {
                options.RootDirectory = "/Packages";
                options.Conventions.Add(new ModularPageRouteModelConvention());
            });

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPageApplicationModelProvider, ModularPageApplicationModelProvider>());

            builder.Services.Replace(
                ServiceDescriptor.Singleton<IActionDescriptorChangeProvider, ModularPageActionDescriptorChangeProvider>());

            return builder;
        }
    }
}
