using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
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
            builder.AddJsonFormatters();

            builder.AddExtensionsApplicationParts(applicationServices);

            var extensionLibraryService = applicationServices.GetRequiredService<IExtensionLibraryService>();
            builder.AddFeatureProvider(
                new ExtensionMetadataReferenceFeatureProvider(extensionLibraryService.MetadataPaths.ToArray()));

            builder.AddRazorViewEngine(options =>
            {
                var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var referencePaths = extensionLibraryService.MetadataPaths;
                foreach (var path in referencePaths)
                {
                    if (libraryPaths.Add(path))
                    {
                        var metadataReference = CreateMetadataReference(path);
                        options.AdditionalCompilationReferences.Add(metadataReference);
                    }
                }

                //var assets = DependencyContext
                //    .Default
                //    .CompileLibraries;

                //foreach (var asset in assets.Where(x => x.Type != "project" && x.Assemblies.Any()))
                //{
                //    if (libraryPaths.Add(asset.Assemblies.First()))
                //    {
                //        Unre
                //        var metadataReference = CreateMetadataReference(asset.Assemblies.First());
                //        options.AdditionalCompilationReferences.Add(metadataReference);
                //    }
                //}

                options.ViewLocationExpanders.Add(new CompositeViewLocationExpanderProvider());
            });

            services.AddScoped<ITenantRouteBuilder, MvcTenantRouteBuilder>();
            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IApplicationModelProvider, ModuleAreaRouteConstraintApplicationModelProvider>();

            services.AddScoped<IViewLocationExpanderProvider, DefaultViewLocationExpanderProvider>();
            services.AddScoped<IViewLocationExpanderProvider, ModuleViewLocationExpanderProvider>();


            return services;
        }

        public static IMvcCoreBuilder AddExtensionsApplicationParts(this IMvcCoreBuilder builder,
            IServiceProvider applicationServices)
        {
            var extensionManager = applicationServices.GetRequiredService<IExtensionManager>();

            var availableExtensions = extensionManager.GetExtensions();

            ConcurrentBag<Assembly> bagOfAssemblies = new ConcurrentBag<Assembly>();
            Parallel.ForEach(availableExtensions, new ParallelOptions { MaxDegreeOfParallelism = 4 }, ae =>
            {
                var extensionEntry = extensionManager
                    .LoadExtensionAsync(ae)
                    .GetAwaiter()
                    .GetResult();

                if (!extensionEntry.IsError)
                {
                    bagOfAssemblies.Add(extensionEntry.Assembly);
                }
            });

            foreach (var ass in bagOfAssemblies)
            {
                var tpyeso = ass.DefinedTypes.Select(x => x.Assembly);

                builder.AddApplicationPart(ass);
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

            builder.ConfigureApplicationPartManager(manager => manager.FeatureProviders.Insert(0, provider));

            return builder;
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