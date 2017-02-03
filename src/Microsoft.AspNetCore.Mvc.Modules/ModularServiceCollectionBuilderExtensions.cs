using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Modules.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Modules.LocationExpander;
using Microsoft.AspNetCore.Mvc.Modules.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Environment.Extensions;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public static class ModularServiceCollectionBuilderExtensions
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
            var builder = services.AddMvcCore(options => {
                // Do we need this?
                options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));

                options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
            });

            builder.AddViews();
            builder.AddViewLocalization();

            AddModularFrameworkParts(applicationServices, builder.PartManager);

            builder.AddModularRazorViewEngine(applicationServices);

            AddMvcModuleCoreServices(services);
            AddDefaultFrameworkParts(builder.PartManager);

            // Order important
            builder.AddJsonFormatters();

            return services;
        }

        internal static void AddModularFrameworkParts(IServiceProvider services, ApplicationPartManager manager)
        {
            var httpContextAccessor =
                services.GetRequiredService<IHttpContextAccessor>();

            manager.ApplicationParts.Add(new ModularApplicationPart(httpContextAccessor));
        }

        private static void AddDefaultFrameworkParts(ApplicationPartManager partManager)
        {
            var mvcTagHelpersAssembly = typeof(InputTagHelper).GetTypeInfo().Assembly;
            if (!partManager.ApplicationParts.OfType<AssemblyPart>().Any(p => p.Assembly == mvcTagHelpersAssembly))
            {
                partManager.ApplicationParts.Add(new AssemblyPart(mvcTagHelpersAssembly));
            }

            var mvcRazorAssembly = typeof(UrlResolutionTagHelper).GetTypeInfo().Assembly;
            if (!partManager.ApplicationParts.OfType<AssemblyPart>().Any(p => p.Assembly == mvcRazorAssembly))
            {
                partManager.ApplicationParts.Add(new AssemblyPart(mvcRazorAssembly));
            }
        }

        internal static IMvcCoreBuilder AddModularRazorViewEngine(this IMvcCoreBuilder builder, IServiceProvider services)
        {
            return builder.AddRazorViewEngine(options =>
            {
                var extensionLibraryService = 
                    services.GetService<IExtensionLibraryService>();

                foreach (var metadataPath in extensionLibraryService.MetadataPaths)
                {
                    var metadata = CreateMetadataReference(metadataPath);
                    options.AdditionalCompilationReferences.Add(metadata);
                }

                options.ViewLocationExpanders.Add(new CompositeViewLocationExpanderProvider());
            });
        }
        
        internal static void AddMvcModuleCoreServices(IServiceCollection services)
        {
            services.AddScoped<ITenantRouteBuilder, ModularRouteBuilder>();

            services.AddScoped<IViewLocationExpanderProvider, DefaultViewLocationExpanderProvider>();
            services.AddScoped<IViewLocationExpanderProvider, ModulerViewLocationExpanderProvider>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ModularApplicationModelProvider>());
            services.Replace(
                ServiceDescriptor.Transient<ITagHelperTypeResolver, FeatureTagHelperTypeResolver>());
        }

        private static MetadataReference CreateMetadataReference(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                return assemblyMetadata.GetReference(filePath: path);
            }
        }
    }
}