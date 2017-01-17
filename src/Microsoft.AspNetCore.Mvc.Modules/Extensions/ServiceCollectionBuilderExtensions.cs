using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Modules.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Modules.Mvc;
using Microsoft.AspNetCore.Mvc.Modules.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Modules.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Modules.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public static class ServiceCollectionBuilderExtensions
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
            var builder = services
                .AddMvcCore(options =>
                {
                    options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));
                    options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
                });

            builder.AddViews();
            builder.AddViewLocalization();
            builder.AddRazorViewEngine();
            builder.AddJsonFormatters();

            builder.AddExtensionsApplicationParts(applicationServices);

            var extensionLibraryService = applicationServices.GetRequiredService<IExtensionLibraryService>();
            builder.AddFeatureProvider(
                new ExtensionMetadataReferenceFeatureProvider(extensionLibraryService.MetadataPaths.ToArray()));

            services.AddSingleton<ITenantRouteBuilder, MvcTenantRouteBuilder>();
            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IApplicationModelProvider, ModuleAreaRouteConstraintApplicationModelProvider>();

            return services;
        }

        public static IMvcCoreBuilder AddExtensionsApplicationParts(this IMvcCoreBuilder builder, IServiceProvider applicationServices)
        {
            var extensionManager = applicationServices.GetRequiredService<IExtensionManager>();
            var loggerFactory = applicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Default");

            var availableExtensions = extensionManager.GetExtensions();
            using (logger.BeginScope("Loading extensions"))
            {
                ConcurrentBag<Assembly> bagOfAssemblies = new ConcurrentBag<Assembly>();
                Parallel.ForEach(availableExtensions, new ParallelOptions { MaxDegreeOfParallelism = 4 }, ae =>
                {
                    try
                    {
                        var extensionEntry = extensionManager
                            .LoadExtensionAsync(ae)
                            .GetAwaiter()
                            .GetResult();

                        if (!extensionEntry.IsError)
                        {
                            bagOfAssemblies.Add(extensionEntry.Assembly);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical("Could not load an extension", ae, e);
                    }
                });

                foreach (var ass in bagOfAssemblies)
                {
                    builder.AddApplicationPart(ass);
                }
            }

            return builder;
        }

        /// <summary>
        /// Adds an <see cref="ApplicationPart"/> to the list of <see cref="ApplicationPartManager.ApplicationParts"/> on the
        /// <see cref="IMvcBuilder.PartManager"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="assembly">The <see cref="Assembly"/> of the <see cref="ApplicationPart"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcCoreBuilder AddFeatureProvider(this IMvcCoreBuilder builder, IApplicationFeatureProvider provider)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            builder.ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(provider));

            return builder;
        }
    }
}