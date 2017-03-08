using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    public class ShapeTemplateBindingStrategy : IShapeTableHarvester
    {
        private readonly IEnumerable<IShapeTemplateHarvester> _harvesters;
        private readonly IEnumerable<IShapeTemplateViewEngine> _shapeTemplateViewEngines;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;
        private readonly IShellFeaturesManager _shellFeaturesManager;

        private readonly Dictionary<string, IShapeTemplateViewEngine> _viewEnginesByExtension =
            new Dictionary<string, IShapeTemplateViewEngine>(StringComparer.OrdinalIgnoreCase);

        public ShapeTemplateBindingStrategy(
            IEnumerable<IShapeTemplateHarvester> harvesters,
            IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IShapeTemplateViewEngine> shapeTemplateViewEngines,
            IOptions<MvcViewOptions> options,
            IHostingEnvironment hostingEnvironment,
            ILogger<DefaultShapeTableManager> logger)
        {
            _harvesters = harvesters;
            _shellFeaturesManager = shellFeaturesManager;
            _shapeTemplateViewEngines = shapeTemplateViewEngines;
            _hostingEnvironment = hostingEnvironment;
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

            var matcher = new Matcher();

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

            foreach (var extension in _viewEnginesByExtension.Keys)
            {
                matcher.AddInclude("*" + extension);
            }

            var hits = activeExtensions.Select(extensionDescriptor =>
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Start discovering candidate views filenames");
                }

                var pathContexts = harvesterInfos.SelectMany(harvesterInfo => harvesterInfo.subPaths.Select(subPath =>
                {
                    var subPathFileInfo = _hostingEnvironment
                        .GetExtensionFileInfo(extensionDescriptor, subPath);

                    var directoryInfo = new DirectoryInfo(subPathFileInfo.PhysicalPath);

                    var relativePath = Path.Combine(extensionDescriptor.SubPath, subPath);

                    if (!directoryInfo.Exists)
                    {
                        return new
                        {
                            harvesterInfo.harvester,
                            subPath,
                            relativePath,
                            files = new IFileInfo[0]
                        };
                    }

                    var matches = matcher
                        .Execute(new DirectoryInfoWrapper(directoryInfo))
                        .Files;

                    var files = matches
                        .Select(match => _hostingEnvironment
                            .GetExtensionFileInfo(extensionDescriptor, Path.Combine(subPath, match.Path))).ToArray();

                    return new { harvesterInfo.harvester, subPath, relativePath, files };
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
                            relativePath = Path.Combine(pathContext.relativePath, file.Name),
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
                        _logger.LogDebug("Binding {0} as shape [{1}] for feature {2}",
                            hit.shapeContext.harvestShapeInfo.RelativePath,
                            iter.shapeContext.harvestShapeHit.ShapeType,
                            feature.Id);
                    }

                    var viewEngine = _viewEnginesByExtension[iter.shapeContext.harvestShapeInfo.Extension];

                    builder.Describe(iter.shapeContext.harvestShapeHit.ShapeType)
                        .From(feature)
                        .BoundAs(
                            hit.shapeContext.harvestShapeInfo.RelativePath, shapeDescriptor => displayContext =>
                                viewEngine.RenderAsync(hit.shapeContext.harvestShapeInfo.RelativePath, displayContext));
                }
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done discovering shapes");
            }
        }
    }
}