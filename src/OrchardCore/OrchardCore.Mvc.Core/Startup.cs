using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Mvc.LocationExpander;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.RazorPages;

namespace OrchardCore.Mvc
{
    public class Startup : StartupBase
    {
        public override int Order => -200;

        private readonly IServiceProvider _serviceProvider;

        public Startup(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddMvc(options =>
            {
                // Forcing AntiForgery Token Validation on by default, it's only in Razor Pages by default
                options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));

                // Custom model binder to testing purpose
                options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
            });

            builder.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Latest);

            services.AddMvcCore().AddModularRazorPages();

            AddModularFrameworkParts(_serviceProvider, builder.PartManager);
            
            // Adding localization
            builder.AddViewLocalization();
            builder.AddDataAnnotationsLocalization();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>, ModularRazorViewEngineOptionsSetup>());
            
            // Use a custom IViewCompilerProvider so that all tenants reuse the same ICompilerCache instance
            builder.Services.Replace(new ServiceDescriptor(typeof(IViewCompilerProvider), typeof(SharedViewCompilerProvider), ServiceLifetime.Singleton));

            AddMvcModuleCoreServices(services);
        }

        private void AddModularFrameworkParts(IServiceProvider services, ApplicationPartManager manager)
        {
            var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            manager.ApplicationParts.Add(new ShellFeatureApplicationPart(httpContextAccessor));
        }
        
        internal static void AddMvcModuleCoreServices(IServiceCollection services)
        {
            services.Replace(
                ServiceDescriptor.Transient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>());

            services.AddScoped<IViewLocationExpanderProvider, ComponentViewLocationExpanderProvider>();

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IApplicationModelProvider, ModularApplicationModelProvider>());
        }
    }
}
