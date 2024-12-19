using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Mvc.LocationExpander;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.RazorPages;
using OrchardCore.Mvc.Routing;
using OrchardCore.Routing;

namespace OrchardCore.Mvc;

public sealed class Startup : StartupBase
{
    public override int Order => -1000;
    public override int ConfigureOrder => 1000;

    private readonly IHostEnvironment _hostingEnvironment;
    private readonly IServiceProvider _serviceProvider;

    public Startup(IHostEnvironment hostingEnvironment, IServiceProvider serviceProvider)
    {
        _hostingEnvironment = hostingEnvironment;
        _serviceProvider = serviceProvider;
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var descriptors = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>()
            .ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .ToArray();

        var mappers = serviceProvider.GetServices<IAreaControllerRouteMapper>().OrderBy(x => x.Order);

        foreach (var descriptor in descriptors)
        {
            if (!descriptor.RouteValues.ContainsKey("area"))
            {
                continue;
            }

            foreach (var mapper in mappers)
            {
                if (mapper.TryMapAreaControllerRoute(routes, descriptor))
                {
                    break;
                }
            }
        }

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
            options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>(999);

            // Custom model binder to testing purpose
            options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());

            options.ModelBinderProviders.Insert(0, new SafeBoolModelBinderProvider());
        });

        // Add a route endpoint selector policy.
        services.AddSingleton<MatcherPolicy, FormValueRequiredMatcherPolicy>();

        services.AddModularRazorPages();

        AddModularFrameworkParts(_serviceProvider, builder.PartManager);

        // Adding localization
        builder.AddViewLocalization();
        builder.AddDataAnnotationsLocalization();

        services.AddTransient<IConfigureOptions<RazorViewEngineOptions>, ModularRazorViewEngineOptionsSetup>();

        if (_hostingEnvironment.IsDevelopment())
        {
            // Support razor runtime compilation only if in dev mode and if the 'refs' folder exists.
            var refsFolderExists = Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "refs"));

            if (refsFolderExists)
            {
                builder.AddRazorRuntimeCompilation();
            }
        }
        else
        {
            // Share across tenants a static compiler even if there is no runtime compilation
            // because the compiler still uses its internal cache to retrieve compiled items.
            // Register this provider only in production mode, as it may cause hot reload to fail in development mode.
            services.AddSingleton<IViewCompilerProvider, SharedViewCompilerProvider>();
        }

        services.AddTransient<IConfigureOptions<MvcRazorRuntimeCompilationOptions>, RazorCompilationOptionsSetup>();

        services.AddSingleton<RazorCompilationFileProviderAccessor>();

        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

        // Use a custom 'IFileVersionProvider' that also lookup all tenant level 'IStaticFileProvider'.
        services.Replace(ServiceDescriptor.Singleton<IFileVersionProvider, ShellFileVersionProvider>());

        // Register a DefaultAreaControllerRouteMapper that will run last.
        services.AddTransient<IAreaControllerRouteMapper, DefaultAreaControllerRouteMapper>();

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

        services.AddSingleton<IApplicationModelProvider, ModularApplicationModelProvider>();
    }
}
