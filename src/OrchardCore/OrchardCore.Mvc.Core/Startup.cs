using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public override int Order => -200;

        private readonly IServiceProvider _serviceProvider;

        public Startup(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Register an isolated tenant part manager.
            services.AddSingleton(new ApplicationPartManager());

            var builder = services.AddMvc(options =>
            {
                // Forcing AntiForgery Token Validation on by default, it's only in Razor Pages by default
                options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAttribute));

                // Custom model binder to testing purpose
                options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
            });

            builder.SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddModularRazorPages();

            AddModularFrameworkParts(_serviceProvider, builder.PartManager);

            // Adding localization
            builder.AddViewLocalization();
            builder.AddDataAnnotationsLocalization();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>, ModularRazorViewEngineOptionsSetup>());

            // Use a custom 'IViewCompilationMemoryCacheProvider' so that all tenants reuse the same ICompilerCache instance.
            services.AddSingleton<IViewCompilationMemoryCacheProvider>(new RazorViewCompilationMemoryCacheProvider());

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Use a custom 'IFileVersionProvider' that also lookup all tenant level 'IStaticFileProvider'.
            services.Replace(ServiceDescriptor.Singleton<IFileVersionProvider, ShellFileVersionProvider>());

            AddMvcModuleCoreServices(services);
        }

        private void AddModularFrameworkParts(IServiceProvider services, ApplicationPartManager manager)
        {
            manager.ApplicationParts.Insert(0, new ShellFeatureApplicationPart());
            manager.FeatureProviders.Add(new ShellViewFeatureProvider(services));
        }

        internal static void AddMvcModuleCoreServices(IServiceCollection services)
        {
            services.Replace(
                ServiceDescriptor.Transient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>());

            services.AddScoped<IViewLocationExpanderProvider, ComponentViewLocationExpanderProvider>();

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IApplicationModelProvider, ModularApplicationModelProvider>());
        }

        internal class RazorViewCompilationMemoryCacheProvider : IViewCompilationMemoryCacheProvider
        {
            public IMemoryCache CompilationMemoryCache { get; } = _cache;
        }
    }
}
