using System;
using System.Collections.Generic;
using System.Linq;
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
using Orchard.Environment.Extensions.Manifests;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Hosting;
using Orchard.Hosting.Mvc.Filters;
using Orchard.Hosting.Mvc.ModelBinding;
using Orchard.Hosting.Routing;

namespace Microsoft.AspNetCore.Mvc.Modules.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModuleServices(this IServiceCollection services)
        {
            return AddModuleServices(services, null, "Module.txt");
        }

        public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration)
        {
            return AddModuleServices(services, configuration, "Module.txt");
        }

        public static IServiceCollection AddModuleServices(this IServiceCollection services, string manifestFileName)
        {
            return AddModuleServices(services, null, manifestFileName);
        }

        public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration, string manifestFileName)
        {
            return AddModuleServices(services, configuration, manifestFileName, null);
        }

        public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration, string manifestFileName, Action<MvcOptions> mvcSetupAction)
        {
            services.AddSingleton(new ShellFeature("Orchard.Hosting"));
            services.AddWebHost();
            services.AddManifestDefinition(manifestFileName, "module");
            services.AddExtensionLocation("Modules");

            services
                .AddMvcCore(options =>
                {
                    options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));
                    options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());

                    mvcSetupAction?.Invoke(options);
                })
                .AddAuthorization()
                .AddViews()
                .AddViewLocalization()
                .AddRazorViewEngine()
                .AddJsonFormatters();

            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IApplicationModelProvider, ModuleAreaRouteConstraintApplicationModelProvider>();

            services.Configure<RazorViewEngineOptions>(configureOptions: options =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var extensionLibraryService = serviceProvider.GetService<IExtensionLibraryService>();

                ((List<MetadataReference>)options.AdditionalCompilationReferences).AddRange(extensionLibraryService.MetadataReferences());

                (serviceProvider as IDisposable)?.Dispose();
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

        public static IServiceCollection WithDefaultFeatures(this IServiceCollection services, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                services.AddTransient(sp => new ShellFeature(featureId));
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
                internalServices.AddExtensionManagerHost("App_Data", "dependencies");
                internalServices.AddCommands();

                internalServices.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            });
        }
    }
}
