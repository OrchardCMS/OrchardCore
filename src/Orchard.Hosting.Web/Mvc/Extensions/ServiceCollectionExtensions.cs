using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.TagHelpers;
using Orchard.Environment.Extensions;
using Orchard.Hosting.Mvc.Filters;
using Orchard.Hosting.Mvc.ModelBinding;
using Orchard.Hosting.Mvc.Razor;
using Orchard.Hosting.Routing;
using Orchard.Hosting.Web.Mvc.ModelBinding;

namespace Orchard.Hosting.Mvc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrchardMvc(this IServiceCollection services)
        {
            services
                .AddMvcCore(options =>
                {
                    options.Filters.Add(new ModelBinderAccessorFilter());
                    options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
                })
                .AddViews()
                .AddViewLocalization()
                .AddRazorViewEngine()
                .AddJsonFormatters();

            services.AddScoped<IModelUpdaterAccessor, LocalModelBinderAccessor>();
            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IMvcRazorHost, TagHelperMvcRazorHost>();
            services.AddTransient<IApplicationModelProvider, ModuleAreaRouteConstraintApplicationModelProvider>();

            services.Configure<RazorViewEngineOptions>(configureOptions: options =>
            {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);

                var extensionLibraryService = services.BuildServiceProvider().GetService<IExtensionLibraryService>();
                ((List<MetadataReference>)options.AdditionalCompilationReferences).AddRange(extensionLibraryService.MetadataReferences());
            });

            return services;
        }

    }
}