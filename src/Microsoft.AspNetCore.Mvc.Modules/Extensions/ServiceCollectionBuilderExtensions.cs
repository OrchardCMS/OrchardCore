using System;
using System.IO;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Modules.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Modules.LocationExpander;
using Microsoft.AspNetCore.Mvc.Modules.Mvc;
using Microsoft.AspNetCore.Mvc.Modules.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Modules.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Modules.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

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
            var builder = services.AddMvcCore(options => {
                options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAuthorizationFilter));
                options.ModelBinderProviders.Insert(0, new CheckMarkModelBinderProvider());
            });

            builder.AddViews();
            builder.AddViewLocalization();

            AddModularFrameworkParts(applicationServices, builder.PartManager);

            builder.AddModularRazorViewEngine();

            AddMvcModuleCoreServices(services);

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

        internal static IMvcCoreBuilder AddModularRazorViewEngine(this IMvcCoreBuilder builder)
        {
            return builder.AddRazorViewEngine(options =>
            {


                //// Really need to see how to do a Shell Version of this, as this loads everything.
                //var assemblies = GetModularAssemblies(applicationServices);

                //var loadedContextAssemblies = new List<Assembly>();
                //var assemblyNames = new HashSet<string>();

                //foreach (var assembly in assemblies)
                //{
                //    var currentAssemblyName =
                //        Path.GetFileNameWithoutExtension(assembly.Location);

                //    if (assemblyNames.Add(currentAssemblyName))
                //    {
                //        loadedContextAssemblies.Add(assembly);
                //    }
                //    loadedContextAssemblies.AddRange(GetAssemblyLocations(assemblyNames, assembly));
                //}

                //builder.ConfigureApplicationPartManager((manager) =>
                //{
                //    foreach (var assembly in loadedContextAssemblies)
                //    {
                //        builder.AddApplicationPart(assembly);
                //    }
                //});





                //var orderedLibraryPaths = loadedContextAssemblies
                //    .Select(x => x.Location);

                //foreach (var location in orderedLibraryPaths)
                //{
                //    var metadataReference = CreateMetadataReference(location);
                //    options.AdditionalCompilationReferences.Add(metadataReference);
                //}

                options.ViewLocationExpanders.Add(new CompositeViewLocationExpanderProvider());
            });
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

            builder.ConfigureApplicationPartManager(manager => manager.FeatureProviders.Insert(0, provider));

            return builder;
        }

        internal static void AddMvcModuleCoreServices(IServiceCollection services)
        {
            services.AddScoped<ITenantRouteBuilder, MvcTenantRouteBuilder>();
            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IApplicationModelProvider, ModuleAreaRouteConstraintApplicationModelProvider>();

            services.AddScoped<IViewLocationExpanderProvider, DefaultViewLocationExpanderProvider>();
            services.AddScoped<IViewLocationExpanderProvider, ModuleViewLocationExpanderProvider>();
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