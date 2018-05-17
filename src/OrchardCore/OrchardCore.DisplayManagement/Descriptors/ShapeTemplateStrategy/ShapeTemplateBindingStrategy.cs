using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    public class ShapeTemplateBindingStrategy : IShapeTableHarvester
    {
        private readonly string _shellName;
        private readonly IEnumerable<IShapeTemplateHarvester> _harvesters;
        private readonly IEnumerable<IShapeTemplateViewEngine> _shapeTemplateViewEngines;
        private readonly IShapeTemplateFileProviderAccessor _fileProviderAccessor;
        private readonly ILogger _logger;
        private readonly IShellFeaturesManager _shellFeaturesManager;

        private readonly Dictionary<string, IShapeTemplateViewEngine> _viewEnginesByExtension =
            new Dictionary<string, IShapeTemplateViewEngine>(StringComparer.OrdinalIgnoreCase);

        public ShapeTemplateBindingStrategy(
            ShellSettings shellSettings,
            IEnumerable<IShapeTemplateHarvester> harvesters,
            IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IShapeTemplateViewEngine> shapeTemplateViewEngines,
            IOptions<MvcViewOptions> options,
            IShapeTemplateFileProviderAccessor fileProviderAccessor,
            ILogger<DefaultShapeTableManager> logger)
        {
            _shellName = shellSettings.Name;
            _harvesters = harvesters;
            _shellFeaturesManager = shellFeaturesManager;
            _shapeTemplateViewEngines = shapeTemplateViewEngines;
            _fileProviderAccessor = fileProviderAccessor;
            _logger = logger;
        }

        public bool DisableMonitoring { get; set; }

        private static IEnumerable<IExtensionInfo> Once(IEnumerable<IFeatureInfo> featureDescriptors)
        {
            var once = new ConcurrentDictionary<string, object>();
            return featureDescriptors.Select(x => x.Extension).Where(ed => once.TryAdd(ed.Id, null)).ToList();
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

            var enabledFeatures = _shellFeaturesManager.GetEnabledFeaturesAsync().GetAwaiter().GetResult()
                .Where(Feature => !builder.ExcludedFeatureIds.Contains(Feature.Id)).ToList();

            var activeExtensions = Once(enabledFeatures);

            if (!_viewEnginesByExtension.Any())
            {
                foreach (var viewEngine in _shapeTemplateViewEngines)
                {
                    foreach (var extension in viewEngine.TemplateFileExtensions)
                    {
                        if (!_viewEnginesByExtension.ContainsKey(extension))
                        {
                            _viewEnginesByExtension[extension] = viewEngine;
                        }
                    }
                }
            }

            var hits = activeExtensions.Select(extensionDescriptor =>
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Start discovering candidate views filenames");
                }

                var pathContexts = harvesterInfos.SelectMany(harvesterInfo => harvesterInfo.subPaths.Select(subPath =>
                {
                    var filePaths = _fileProviderAccessor.FileProvider.GetViewFilePaths(Path.Combine(
                        extensionDescriptor.SubPath, subPath), _viewEnginesByExtension.Keys.ToArray(),
                        inViewsFolder: true, inDepth: false).ToArray();

                    return new { harvesterInfo.harvester, subPath, filePaths };
                }))
                .ToList();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Done discovering candidate views filenames");
                }

                var fileContexts = pathContexts.SelectMany(pathContext => _shapeTemplateViewEngines.SelectMany(ve =>
                {
                    return pathContext.filePaths.Select(
                        filePath => new
                        {
                            fileName = Path.GetFileNameWithoutExtension(filePath),
                            relativePath = filePath,
                            pathContext
                        });
                }));

                var shapeContexts = fileContexts.SelectMany(fileContext =>
                {
                    var harvestShapeInfo = new HarvestShapeInfo
                    {
                        SubPath = fileContext.pathContext.subPath,
                        FileName = fileContext.fileName,
                        RelativePath = fileContext.relativePath,
                        Extension = Path.GetExtension(fileContext.relativePath)
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
                foreach (var feature in hit.extensionDescriptor.Features)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Binding '{TemplatePath}' as shape '{ShapeType}' for feature '{FeatureName}'",
                            hit.shapeContext.harvestShapeInfo.RelativePath,
                            iter.shapeContext.harvestShapeHit.ShapeType,
                            feature.Id);
                    }

                    var viewEngineType = _viewEnginesByExtension[iter.shapeContext.harvestShapeInfo.Extension].GetType();

                    builder.Describe(iter.shapeContext.harvestShapeHit.ShapeType)
                        .From(feature)
                        .BoundAs(
                            hit.shapeContext.harvestShapeInfo.RelativePath, shapeDescriptor => displayContext =>
                            {
                                var viewEngine = displayContext.ServiceProvider
                                    .GetServices<IShapeTemplateViewEngine>()
                                    .FirstOrDefault(e => e.GetType() == viewEngineType);

                                return viewEngine.RenderAsync(hit.shapeContext.harvestShapeInfo.RelativePath, displayContext);
                            });
                }
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done discovering shapes");
            }
        }
    }
}