using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;
using OrchardCore.Mvc.LocationExpander;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.RazorPages;

namespace OrchardCore.Mvc
{
    public static class ModularServiceCollectionExtensions
    {
        public static ModularServiceCollection AddMvcModules(this ModularServiceCollection moduleServices,
            IServiceProvider applicationServices)
        {
            moduleServices.Configure(services =>
            {
                services.AddMvcModules(applicationServices);
            });

            return moduleServices;
        }

        public static IServiceCollection AddMvcModules(this IServiceCollection services,
            IServiceProvider applicationServices)
        {
            services.TryAddSingleton(new ApplicationPartManager());

            var builder = services.AddMvcCore(options =>
            {
                // Do we need this?
                options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));
                options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
            });

            builder.AddAuthorization();
            builder.AddViews();
            builder.AddViewLocalization();

            AddModularFrameworkParts(applicationServices, builder.PartManager);

            builder.AddModularRazorViewEngine(applicationServices);
            builder.AddModularRazorPages(applicationServices);

            // Use a custom IViewCompilerProvider so that all tenants reuse the same ICompilerCache instance
            builder.Services.Replace(new ServiceDescriptor(typeof(IViewCompilerProvider), typeof(SharedViewCompilerProvider), ServiceLifetime.Singleton));

            AddMvcModuleCoreServices(services);
            AddDefaultFrameworkParts(builder.PartManager);

            // Order important
            builder.AddJsonFormatters();

            return services;
        }

        public static void AddTagHelpers(this IServiceProvider serviceProvider, string assemblyName)
        {
            serviceProvider.AddTagHelpers(Assembly.Load(new AssemblyName(assemblyName)));
        }

        public static void AddTagHelpers(this IServiceProvider serviceProvider, Assembly assembly)
        {
            serviceProvider.GetRequiredService<ApplicationPartManager>()
                .ApplicationParts.Add(new TagHelperApplicationPart(assembly));
        }

        internal static void AddModularFrameworkParts(IServiceProvider services, ApplicationPartManager manager)
        {
            var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            manager.ApplicationParts.Add(new ShellFeatureApplicationPart(httpContextAccessor));
        }

        private static void AddDefaultFrameworkParts(ApplicationPartManager partManager)
        {
            var mvcTagHelpersAssembly = typeof(InputTagHelper).Assembly;
            if (!partManager.ApplicationParts.OfType<AssemblyPart>().Any(p => p.Assembly == mvcTagHelpersAssembly))
            {
                partManager.ApplicationParts.Add(new AssemblyPart(mvcTagHelpersAssembly));
            }

            var mvcRazorAssembly = typeof(UrlResolutionTagHelper).Assembly;
            if (!partManager.ApplicationParts.OfType<AssemblyPart>().Any(p => p.Assembly == mvcRazorAssembly))
            {
                partManager.ApplicationParts.Add(new AssemblyPart(mvcRazorAssembly));
            }
        }

        internal static IMvcCoreBuilder AddModularRazorViewEngine(this IMvcCoreBuilder builder, IServiceProvider services)
        {
            var env = services.GetRequiredService<IHostingEnvironment>();

            return builder.AddRazorViewEngine(options =>
            {
                options.ViewLocationExpanders.Add(new CompositeViewLocationExpanderProvider());

                if (env.IsDevelopment())
                {
                    options.FileProviders.Insert(0, new ModuleProjectRazorFileProvider(env));
                }
            });
        }

        internal static void AddMvcModuleCoreServices(IServiceCollection services)
        {
            services.Replace(
                ServiceDescriptor.Scoped<IModularTenantRouteBuilder, ModularTenantRouteBuilder>());

            services.AddScoped<IViewLocationExpanderProvider, DefaultViewLocationExpanderProvider>();
            services.AddScoped<IViewLocationExpanderProvider, ModularViewLocationExpanderProvider>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ModularApplicationModelProvider>());
        }
    }
}
