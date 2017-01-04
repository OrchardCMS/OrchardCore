using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Hosting.Mvc.Filters;
using Orchard.Hosting.Mvc.ModelBinding;
using Orchard.Hosting.Routing;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public static class ServiceCollectionBuilderExtensions
    {
        public static ModularServiceCollection AddMvcModules(this ModularServiceCollection moduleServices)
        {
            moduleServices.Configure(services =>
            {
                services
                    .AddMvcCore(options =>
                    {
                        options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));
                        options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
                    })
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
            });

            return moduleServices;
        }
    }
}