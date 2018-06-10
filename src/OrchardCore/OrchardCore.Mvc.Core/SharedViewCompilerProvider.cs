using System;
using System.Collections.Generic;
using System.Globalization;
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
        private object _initializeLock = new object();
        private bool _initialized;

        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IRazorViewEngineFileProviderAccessor _fileProviderAccessor;
        private readonly IEnumerable<IApplicationFeatureProvider<ViewsFeature>> _viewsFeatureProviders;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly RazorTemplateEngine _razorTemplateEngine;
        private readonly CSharpCompiler _csharpCompiler;
        private readonly RazorViewEngineOptions _viewEngineOptions;
        private readonly ILogger<SharedViewCompilerProvider> _logger;
        private IViewCompiler _compiler;


        public SharedViewCompilerProvider(
            ApplicationPartManager applicationPartManager,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IEnumerable<IApplicationFeatureProvider<ViewsFeature>> viewsFeatureProviders,
            IHostingEnvironment hostingEnvironment,
            RazorTemplateEngine razorTemplateEngine,
            CSharpCompiler csharpCompiler,
            IOptions<RazorViewEngineOptions> viewEngineOptionsAccessor,
            ILoggerFactory loggerFactory)
        {
            _applicationPartManager = applicationPartManager;
            _fileProviderAccessor = fileProviderAccessor;
            _viewsFeatureProviders = viewsFeatureProviders;
            _hostingEnvironment = hostingEnvironment;
            _razorTemplateEngine = razorTemplateEngine;
            _csharpCompiler = csharpCompiler;
            _viewEngineOptions = viewEngineOptionsAccessor.Value;
            _logger = loggerFactory.CreateLogger<SharedViewCompilerProvider>();
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
                CreateCompiler);
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
                    !descriptor.ViewAttribute.ViewType.Name.EndsWith("_cshtml"))
                {
                    continue;
                }

                descriptor.RelativePath = '/' + application.ModulePath + descriptor.RelativePath;
            }

            return new SharedRazorViewCompiler(
                _fileProviderAccessor.FileProvider,
                _razorTemplateEngine,
                _csharpCompiler,
                _viewEngineOptions.CompilationCallback,
                feature.ViewDescriptors,
                _logger);
        }
    }
}