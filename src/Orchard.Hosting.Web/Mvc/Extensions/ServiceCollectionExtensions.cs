using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.TagHelpers;
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
                    options.Conventions.Add(new ModuleAreaRouteConstraintConvention());
                    options.ModelBinders.Insert(0, new CheckMarkModelBinder());
                })
                .AddViews()
                .AddViewLocalization()
                .AddRazorViewEngine()
                .AddJsonFormatters();

            services.AddScoped<IModelUpdaterAccessor, LocalModelBinderAccessor>();
            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IMvcRazorHost, TagHelperMvcRazorHost>();

            services.AddScoped<IAssemblyProvider, OrchardMvcAssemblyProvider>();

            services.AddSingleton<ICompilationService, DefaultRoslynCompilationService>();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });
            return services;
        }
    }
}