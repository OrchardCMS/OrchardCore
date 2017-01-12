using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Modules.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Modules.Mvc;
using Microsoft.AspNetCore.Mvc.Modules.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Modules.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Modules.Routing;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public static class ServiceCollectionBuilderExtensions
    {
        public static ModularServiceCollection AddMvcModules(this ModularServiceCollection moduleServices)
        {
            moduleServices.Configure(services =>
            {
                services.AddMvcModules();
            });

            return moduleServices;
        }

        public static IServiceCollection AddMvcModules(this IServiceCollection services)
        {
            services.AddScoped<ITenantRouteBuilder, MvcTenantRouteBuilder>();

            services
                .AddMvcCore(options =>
                {
                    options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));
                    options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
                })
                .AddViews()
                .AddViewLocalization()
                .AddRazorViewEngine()
                .AddJsonFormatters()
                .ConfigureApplicationPartManager(apm =>
                {
                    var provider = services.BuildServiceProvider();

                    var extensionManager = provider.GetRequiredService<IExtensionManager>();
                    var hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("Default");

                    var availableExtensions = extensionManager.GetExtensions();
                    using (logger.BeginScope("Loading extensions"))
                    {
                        ConcurrentBag<ApplicationPart> applicationParts
                            = new ConcurrentBag<ApplicationPart>();

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
                                    applicationParts.Add(new AssemblyPart(extensionEntry.Assembly));
                                }
                            }
                            catch (Exception e)
                            {
                                logger.LogCritical("Could not load an extension", ae, e);
                            }
                        });

                        foreach (var ap in applicationParts) {
                            apm.ApplicationParts.Add(ap);
                        }
                    }

                    var extensionLibraryService = provider.GetRequiredService<IExtensionLibraryService>();
                    apm.FeatureProviders.Add(new ExtensionMetadataReferenceFeatureProvider(extensionLibraryService));
                });

            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IApplicationModelProvider, ModuleAreaRouteConstraintApplicationModelProvider>();

            return services;
        }
    }
}