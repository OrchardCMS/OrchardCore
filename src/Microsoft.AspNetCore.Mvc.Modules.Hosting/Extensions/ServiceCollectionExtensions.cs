using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Commands;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Hosting;
using Orchard.Hosting.Mvc.Filters;
using Orchard.Hosting.Mvc.ModelBinding;
using Orchard.Hosting.Mvc.Razor;
using Orchard.Hosting.Routing;

namespace Microsoft.AspNetCore.Mvc.Modules.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModuleServices(this IServiceCollection services)
        {
            return AddModuleServices(services, null, "Modules", null);
        }

        public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration)
        {
            return AddModuleServices(services, configuration, "Modules", null);
        }

        public static IServiceCollection AddModuleServices(this IServiceCollection services, string modulesPath)
        {
            return AddModuleServices(services, null, modulesPath, null);
        }

        public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration, string modulesPath)
        {
            return AddModuleServices(services, configuration, modulesPath, null);
        }

        public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration, string modulesPath, Action<MvcOptions> mvcSetupAction)
        {
            services.AddSingleton(new ShellFeature("Orchard.Hosting"));
            services.AddWebHost();
            services.AddModuleFolder(modulesPath);

            services
                .AddMvcCore(options =>
                {
                    options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));
                    options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());

                    mvcSetupAction?.Invoke(options);
                })
                .AddViews()
                .AddViewLocalization()
                .AddRazorViewEngine()
                .AddJsonFormatters();

            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IApplicationModelProvider, ModuleAreaRouteConstraintApplicationModelProvider>();

            services.Configure<RazorViewEngineOptions>(configureOptions: options =>
            {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);

                var extensionLibraryService = services.BuildServiceProvider().GetService<IExtensionLibraryService>();
                ((List<MetadataReference>)options.AdditionalCompilationReferences).AddRange(extensionLibraryService.MetadataReferences());
            });

            // Register the configuration object for modules to register options with it
            if (configuration != null)
            {
                services.AddSingleton<IConfiguration>(configuration);
            }

            // Register the list of services to be resolved later on
            services.AddSingleton(_ => services);

            return services;
        }

        public static IServiceCollection WithDefaultFeatures(this IServiceCollection services, params string[] featureNames)
        {
            foreach (var featureName in featureNames)
            {
                services.AddTransient(sp => new ShellFeature(featureName));
            };

            return services;
        }

        /// <summary>
        /// Enables all available features.
        /// </summary>
        public static IServiceCollection WithAllFeatures(this IServiceCollection services)
        {
            return services.AddAllFeaturesDescriptor();
        }

        public static IServiceCollection AddWebHost(this IServiceCollection services)
        {
            return services.AddHost(internalServices =>
            {
                internalServices.AddLogging();
                internalServices.AddOptions();
                internalServices.AddLocalization();
                internalServices.AddHostCore();
                internalServices.AddExtensionManagerHost("app_data", "dependencies");
                internalServices.AddCommands();

                internalServices.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            });
        }
    }
}