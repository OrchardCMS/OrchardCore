using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Modules.Extensions;
using OrchardCore.Mvc.LocationExpander;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.RazorPages;

namespace OrchardCore.Mvc
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level MVC services and configuration.
        /// </summary>
        public static OrchardCoreBuilder AddMvc(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureTenantServices((collection, sp) =>
            {
                AddMvcTenantServices(collection, sp);
            })
            .UseStaticFiles();
        }

        public static void AddMvcTenantServices(IServiceCollection services,
            IServiceProvider serviceProvider)
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

            AddModularFrameworkParts(serviceProvider, builder.PartManager);

            builder.AddModularRazorViewEngine();
            builder.AddModularRazorPages();

            // Use a custom IViewCompilerProvider so that all tenants reuse the same ICompilerCache instance
            builder.Services.Replace(new ServiceDescriptor(typeof(IViewCompilerProvider), typeof(SharedViewCompilerProvider), ServiceLifetime.Singleton));

            AddMvcModuleCoreServices(services);
            AddDefaultFrameworkParts(builder.PartManager);

            // Order important
            builder.AddJsonFormatters();
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

        internal static IMvcCoreBuilder AddModularRazorViewEngine(this IMvcCoreBuilder builder)
        {
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>, ModularRazorViewEngineOptionsSetup>());

            return builder;
        }

        internal static void AddMvcModuleCoreServices(IServiceCollection services)
        {
            services.Replace(
                ServiceDescriptor.Transient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>());

            services.AddScoped<IViewLocationExpanderProvider, DefaultViewLocationExpanderProvider>();
            services.AddScoped<IViewLocationExpanderProvider, ModularViewLocationExpanderProvider>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ModularApplicationModelProvider>());
        }
    }
}
