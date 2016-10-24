using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    public class ShapeTemplateBindingStrategy : IShapeTableProvider
    {
        private readonly IEnumerable<IShapeTemplateHarvester> _harvesters;
        private readonly IEnumerable<IShapeTemplateViewEngine> _shapeTemplateViewEngines;
        private readonly IOptions<MvcViewOptions> _viewEngine;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;
        private readonly IFeatureManager _featureManager;
        private readonly IExtensionManager _extensionManager;

        public ShapeTemplateBindingStrategy(
            IEnumerable<IShapeTemplateHarvester> harvesters,
            IFeatureManager featureManager,
            IExtensionManager extensionManager,
            IEnumerable<IShapeTemplateViewEngine> shapeTemplateViewEngines,
            IOptions<MvcViewOptions> options,
            IHostingEnvironment hostingEnvironment,
            ILogger<DefaultShapeTableManager> logger)
        {
            _harvesters = harvesters;
            _featureManager = featureManager;
            _extensionManager = extensionManager;
            _shapeTemplateViewEngines = shapeTemplateViewEngines;
            _viewEngine = options;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public bool DisableMonitoring { get; set; }

        private static IEnumerable<IFeatureInfo> Once(IEnumerable<IFeatureInfo> featureDescriptors)
        {
            var once = new ConcurrentDictionary<string, object>();
            return featureDescriptors.Where(ed => once.TryAdd(ed.Id, null)).ToList();
        }

        public void Discover(ShapeTableBuilder builder)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Start discovering shapes");
            }

            var harvesterInfos = _harvesters
                .Select(harvester => new { harvester, subPaths = harvester.SubPaths() })
                .ToList();

            var activeFeatures = _featureManager.GetEnabledFeaturesAsync().Result.ToList();
            var activeExtensions = Once(activeFeatures).ToList();

            var matcher = new Matcher();
            foreach (var extension in _shapeTemplateViewEngines.SelectMany(x => x.TemplateFileExtensions))
            {
                matcher.AddInclude(string.Format("*.{0}", extension));
            }

            var hits = activeExtensions.Select(extensionDescriptor =>
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Start discovering candidate views filenames");
                }

                var pathContexts = harvesterInfos.SelectMany(harvesterInfo => harvesterInfo.subPaths.Select(subPath =>
                {
                    var matches = matcher
                        .Execute(new DirectoryInfoWrapper(new DirectoryInfo(extensionDescriptor.Extension.ExtensionFileInfo.PhysicalPath)))
                        .Files;

                    var files = matches
                        .Select(match => _hostingEnvironment
                            .GetExtensionFileInfo(extensionDescriptor.Extension, Path.Combine(subPath, match.Path))).ToArray();

                    var basePath = Path.Combine(extensionDescriptor.Extension.SubPath, extensionDescriptor.Id);
                    var virtualPath = Path.Combine(basePath, subPath);

                    return new { harvesterInfo.harvester, subPath, virtualPath, files };
                })).ToList();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Done discovering candidate views filenames");
                }
                var fileContexts = pathContexts.SelectMany(pathContext => _shapeTemplateViewEngines.SelectMany(ve =>
                {
                    return pathContext.files.Select(
                        file => new
                        {
                            fileName = Path.GetFileNameWithoutExtension(file.Name),
                            fileVirtualPath = "~/" + Path.Combine(pathContext.virtualPath, file.Name),
                            pathContext
                        });
                }));

                var shapeContexts = fileContexts.SelectMany(fileContext =>
                {
                    var harvestShapeInfo = new HarvestShapeInfo
                    {
                        SubPath = fileContext.pathContext.subPath,
                        FileName = fileContext.fileName,
                        TemplateVirtualPath = fileContext.fileVirtualPath
                    };
                    var harvestShapeHits = fileContext.pathContext.harvester.HarvestShape(harvestShapeInfo);
                    return harvestShapeHits.Select(harvestShapeHit => new { harvestShapeInfo, harvestShapeHit, fileContext });
                });

                return shapeContexts.Select(shapeContext => new { extensionDescriptor, shapeContext }).ToList();
            }).SelectMany(hits2 => hits2);


            foreach (var iter in hits)
            {
                // templates are always associated with the namesake feature of module or theme
                var hit = iter;
                var featureDescriptors = iter.extensionDescriptor.Features.Where(fd => fd.Id == hit.extensionDescriptor.Id);
                foreach (var featureDescriptor in featureDescriptors)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Binding {0} as shape [{1}] for feature {2}",
                        hit.shapeContext.harvestShapeInfo.TemplateVirtualPath,
                        iter.shapeContext.harvestShapeHit.ShapeType,
                        featureDescriptor.Id);
                    }
                    builder.Describe(iter.shapeContext.harvestShapeHit.ShapeType)
                        .From(new Feature { Descriptor = featureDescriptor })
                        .BoundAs(
                            hit.shapeContext.harvestShapeInfo.TemplateVirtualPath,
                            shapeDescriptor => displayContext => RenderAsync(shapeDescriptor, displayContext, hit.shapeContext.harvestShapeInfo, hit.shapeContext.harvestShapeHit));
                }
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done discovering shapes");
            }
        }

        private async Task<IHtmlContent> RenderAsync(ShapeDescriptor shapeDescriptor, DisplayContext displayContext, HarvestShapeInfo harvestShapeInfo, HarvestShapeHit harvestShapeHit)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Rendering template file '{0}'", harvestShapeInfo.TemplateVirtualPath);
            }
            IHtmlContent result;

            if (displayContext.ViewContext.View != null)
            {
                var htmlHelper = MakeHtmlHelper(displayContext.ViewContext, displayContext.ViewContext.ViewData);
                result = htmlHelper.Partial(harvestShapeInfo.TemplateVirtualPath, displayContext.Value);
            }
            else
            {
                // If the View is null, it means that the shape is being executed from a non-view origin / where no ViewContext was established by the view engine, but manually.
                // Manually creating a ViewContext works when working with Shape methods, but not when the shape is implemented as a Razor view template.
                // Horrible, but it will have to do for now.
                result = await RenderRazorViewAsync(harvestShapeInfo.TemplateVirtualPath, displayContext);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done rendering template file '{0}'", harvestShapeInfo.TemplateVirtualPath);
            }
            return result;
        }

        private async Task<IHtmlContent> RenderRazorViewAsync(string path, DisplayContext context)
        {
            var viewEngineResult = _viewEngine.Value.ViewEngines.First().FindView(context.ViewContext, path, isMainPage: false);
            if (viewEngineResult.Success)
            {
                var bufferScope = context.ViewContext.HttpContext.RequestServices.GetRequiredService<IViewBufferScope>();
                var viewBuffer = new ViewBuffer(bufferScope, viewEngineResult.ViewName, ViewBuffer.PartialViewPageSize);
                using (var writer = new ViewBufferTextWriter(viewBuffer, context.ViewContext.Writer.Encoding))
                {
                    // Forcing synchronous behavior so users don't have to await templates.
                    var view = viewEngineResult.View;
                    using (view as IDisposable)
                    {
                        var viewContext = new ViewContext(context.ViewContext, viewEngineResult.View, context.ViewContext.ViewData, writer);
                        await viewEngineResult.View.RenderAsync(viewContext);
                        return viewBuffer;
                    }
                }
            }

            return null;
        }

        private static IHtmlHelper MakeHtmlHelper(ViewContext viewContext, ViewDataDictionary viewData)
        {
            var newHelper = viewContext.HttpContext.RequestServices.GetRequiredService<IHtmlHelper>();

            var contextable = newHelper as IViewContextAware;
            if (contextable != null)
            {
                var newViewContext = new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer);
                contextable.Contextualize(newViewContext);
            }

            return newHelper;
        }
    }
}