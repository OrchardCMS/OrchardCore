using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.TagHelpers;
using Orchard.Environment.Extensions;
using Orchard.Hosting.Mvc.Filters;
using Orchard.Hosting.Mvc.ModelBinding;
using Orchard.Hosting.Mvc.Razor;
using Orchard.Hosting.Routing;
using Orchard.Hosting.Web.Mvc.ModelBinding;

namespace Orchard.Hosting.Mvc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrchardMvc(this IServiceCollection services)
        {
            services
                .AddMvcCore(options =>
                {
                    options.Filters.Add(new ModelBinderAccessorFilter());
                    options.Conventions.Add(new ModuleAreaRouteConstraintConvention());
                    options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
                })
                .AddViews()
                .AddViewLocalization()
                .AddRazorViewEngine()
                .AddJsonFormatters();

            services.AddScoped<IModelUpdaterAccessor, LocalModelBinderAccessor>();
            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IMvcRazorHost, TagHelperMvcRazorHost>();

            //if (DnxPlatformServices.Default.LibraryManager != null)
            //{
            //    var partManager = GetApplicationPartManager(services);
            //    var libraryManager = new OrchardLibraryManager(DnxPlatformServices.Default.LibraryManager);
            //    var provider = new OrchardMvcAssemblyProvider(
            //        libraryManager,
            //        DnxPlatformServices.Default.AssemblyLoaderContainer,
            //        new ExtensionAssemblyLoader(
            //            PlatformServices.Default.Application,
            //            DnxPlatformServices.Default.AssemblyLoadContextAccessor,
            //            PlatformServices.Default.Runtime,
            //            libraryManager));

            //    foreach (var assembly in provider.CandidateAssemblies)
            //    {
            //        partManager.ApplicationParts.Add(new AssemblyPart(assembly));
            //    }
            //}

            services.Configure<RazorViewEngineOptions>(configureOptions: options =>
            {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });
            return services;
        }

        private static ApplicationPartManager GetApplicationPartManager(IServiceCollection services)
        {
            var manager = GetServiceFromCollection<ApplicationPartManager>(services)
                ?? new ApplicationPartManager();

            return manager;
        }

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T) services
                .FirstOrDefault(d => d.ServiceType == typeof (T))
                ?.ImplementationInstance;
        }
    }
}