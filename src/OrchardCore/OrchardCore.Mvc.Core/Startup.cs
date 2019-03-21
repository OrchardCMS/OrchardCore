using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Routing;
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

        public override int Order => -1000;
        public override int ConfigureOrder => 1000;

        private readonly IServiceProvider _serviceProvider;

        public Startup(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // The default route is added to each tenant as a template route.
            routes.MapRoute("Default", "{area:exists}/{controller}/{action}/{id?}");
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

                // The endpoint routing system doesn't support IRouter-based extensibility.
                options.EnableEndpointRouting = false;
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

            AddMvcModuleCoreServices(services);
        }

        private void AddModularFrameworkParts(IServiceProvider services, ApplicationPartManager manager)
        {
            var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            manager.ApplicationParts.Insert(0, new ShellFeatureApplicationPart(httpContextAccessor));
            manager.FeatureProviders.Add(new ShellViewFeatureProvider(httpContextAccessor));
        }

        internal static void AddMvcModuleCoreServices(IServiceCollection services)
        {
            services.Replace(
                ServiceDescriptor.Transient<ITenantPipelineBuilder, TenantPipelineBuilder>());

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
