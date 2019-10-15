using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Mvc.LocationExpander;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.RazorPages;
using OrchardCore.Routing;

namespace OrchardCore.Mvc
{
    public class Startup : StartupBase
    {
        public override int Order => -1000;
        public override int ConfigureOrder => 1000;

        private readonly IServiceProvider _serviceProvider;

        public Startup(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // The default route is added to each tenant as a template route.
            routes.MapControllerRoute("Default", "{area:exists}/{controller}/{action}/{id?}");
            routes.MapRazorPages();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Register an isolated tenant part manager.
            services.AddSingleton(new ApplicationPartManager());

            var builder = services.AddMvc(options =>
            {
                // Forcing AntiForgery Token Validation on by default, it's only in Razor Pages by default
                // Load this filter after the MediaSizeFilterLimitAttribute, but before the
                // IgnoreAntiforgeryTokenAttribute. refer : https://github.com/aspnet/AspNetCore/issues/10384
                options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAttribute), 999);

                // Custom model binder to testing purpose
                options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
            });

            // Add a route endpoint selector policy.
            services.AddSingleton<MatcherPolicy, FormValueRequiredMatcherPolicy>();

            // There are some issues when using the default formatters based on
            // System.Text.Json. Here, we manually add JSON.NET based formatters.
            builder.AddNewtonsoftJson();

            builder.SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddModularRazorPages();

            AddModularFrameworkParts(_serviceProvider, builder.PartManager);

            // Adding localization
            builder.AddViewLocalization();
            builder.AddDataAnnotationsLocalization();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>, ModularRazorViewEngineOptionsSetup>());

            // Support razor runtime compilation only if the 'refs' folder exists.
            var refsFolderExists = Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "refs"));

            if (refsFolderExists)
            {
                builder.AddRazorRuntimeCompilation();

                // Shares across tenants the same compiler and its 'IMemoryCache' instance.
                services.AddSingleton<IViewCompilerProvider, SharedViewCompilerProvider>();
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcRazorRuntimeCompilationOptions>, RazorCompilationOptionsSetup>());

            services.AddSingleton<RazorCompilationFileProviderAccessor>();

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Use a custom 'IFileVersionProvider' that also lookup all tenant level 'IStaticFileProvider'.
            services.Replace(ServiceDescriptor.Singleton<IFileVersionProvider, ShellFileVersionProvider>());

            AddMvcModuleCoreServices(services);
        }

        internal static void AddModularFrameworkParts(IServiceProvider services, ApplicationPartManager manager)
        {
            manager.ApplicationParts.Insert(0, new ShellFeatureApplicationPart());
            manager.FeatureProviders.Add(new ShellViewFeatureProvider(services));
        }

        internal static void AddMvcModuleCoreServices(IServiceCollection services)
        {
            services.AddScoped<IViewLocationExpanderProvider, ComponentViewLocationExpanderProvider>();
            services.AddScoped<IViewLocationExpanderProvider, SharedViewLocationExpanderProvider>();

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IApplicationModelProvider, ModularApplicationModelProvider>());
        }
    }
}
