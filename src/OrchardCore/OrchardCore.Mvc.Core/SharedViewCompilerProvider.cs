using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public class SharedViewCompilerProvider : IViewCompilerProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IEnumerable<IApplicationFeatureProvider<ViewsFeature>> _viewsFeatureProviders;

        private readonly RazorProjectEngine _razorProjectEngine;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IRazorViewEngineFileProviderAccessor _fileProviderAccessor;
        private readonly CSharpCompiler _csharpCompiler;
        private readonly RazorViewEngineOptions _viewEngineOptions;
        private readonly ILogger<RazorViewCompiler> _logger;
        private readonly Func<IViewCompiler> _createCompiler;

        private object _initializeLock = new object();
        private bool _initialized;
        private IViewCompiler _compiler;

        public SharedViewCompilerProvider(
            IHostingEnvironment hostingEnvironment,
            IEnumerable<IApplicationFeatureProvider<ViewsFeature>> viewsFeatureProviders,
            ApplicationPartManager applicationPartManager,
            RazorProjectEngine razorProjectEngine,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            CSharpCompiler csharpCompiler,
            IOptions<RazorViewEngineOptions> viewEngineOptionsAccessor,
            ILoggerFactory loggerFactory)
        {
            _hostingEnvironment = hostingEnvironment;
            _viewsFeatureProviders = viewsFeatureProviders;
            _applicationPartManager = applicationPartManager;
            _razorProjectEngine = razorProjectEngine;
            _fileProviderAccessor = fileProviderAccessor;
            _csharpCompiler = csharpCompiler;
            _viewEngineOptions = viewEngineOptionsAccessor.Value;

            _logger = loggerFactory.CreateLogger<RazorViewCompiler>();
            _createCompiler = CreateCompiler;
        }

        public IViewCompiler GetCompiler()
        {
            var fileProvider = _fileProviderAccessor.FileProvider;
            if (fileProvider is NullFileProvider)
            {
                var message = string.Format(CultureInfo.CurrentCulture,
                    "'{0}.{1}' must not be empty. At least one '{2}' is required to locate a view for rendering.",
                    typeof(RazorViewEngineOptions).FullName,
                    nameof(RazorViewEngineOptions.FileProviders),
                    typeof(IFileProvider).FullName);
                throw new InvalidOperationException(message);
            }

            return LazyInitializer.EnsureInitialized(
                ref _compiler,
                ref _initialized,
                ref _initializeLock,
                _createCompiler);
        }

        private IViewCompiler CreateCompiler()
        {
            var feature = new ViewsFeature();

            var featureProviders = _applicationPartManager.FeatureProviders
                .OfType<IApplicationFeatureProvider<ViewsFeature>>()
                .ToList();

            featureProviders.AddRange(_viewsFeatureProviders);

            var assemblyParts =
                new AssemblyPart[]
                {
                    new AssemblyPart(Assembly.Load(new AssemblyName(_hostingEnvironment.ApplicationName)))
                };

            foreach (var provider in featureProviders)
            {
                provider.PopulateFeature(assemblyParts, feature);
            }

            var application = _hostingEnvironment.GetApplication();
            foreach (var descriptor in feature.ViewDescriptors)
            {
                if (descriptor.RelativePath.StartsWith(Application.ModulesRoot) ||
                    !(descriptor.ViewAttribute?.ViewType.Name.StartsWith("Pages_") ?? false))
                {
                    continue;
                }

                descriptor.RelativePath = '/' + application.ModulePath + descriptor.RelativePath;
            }

            if (!_hostingEnvironment.IsDevelopment())
            {
                var moduleNames = _hostingEnvironment.GetApplication().ModuleNames;
                var moduleFeature = new ViewsFeature();

                foreach (var name in moduleNames)
                {
                    var module = _hostingEnvironment.GetModule(name);

                    if (module.Name == application.Name)
                    {
                        continue;
                    }

                    var precompiledAssemblyPath = Path.Combine(Path.GetDirectoryName(module.Assembly.Location),
                        module.Assembly.GetName().Name + ".Views.dll");

                    if (File.Exists(precompiledAssemblyPath))
                    {
                        try
                        {
                            var assembly = Assembly.LoadFile(precompiledAssemblyPath);

                            foreach (var provider in featureProviders)
                            {
                                provider.PopulateFeature(new ApplicationPart[] { new CompiledRazorAssemblyPart(assembly) }, moduleFeature);
                            }

                            foreach (var descriptor in moduleFeature.ViewDescriptors)
                            {
                                descriptor.RelativePath = '/' + module.SubPath + descriptor.RelativePath;
                                feature.ViewDescriptors.Add(descriptor);
                            }

                            moduleFeature.ViewDescriptors.Clear();
                        }
                        catch (FileLoadException)
                        {
                            // Don't throw if assembly cannot be loaded. This can happen if the file is not a managed assembly.
                        }
                    }
                }
            }

            return new SharedRazorViewCompiler(
                _fileProviderAccessor.FileProvider,
                _razorProjectEngine,
                _csharpCompiler,
                _viewEngineOptions.CompilationCallback,
                feature.ViewDescriptors,
                _logger);
        }
    }
}