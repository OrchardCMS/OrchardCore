using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;

public class ShapeTemplateBindingStrategy : ShapeTableProvider, IShapeTableHarvester
{
    private readonly IEnumerable<IShapeTemplateHarvester> _harvesters;
    private readonly IEnumerable<IShapeTemplateViewEngine> _shapeTemplateViewEngines;
    private readonly IShapeTemplateFileProviderAccessor _fileProviderAccessor;
    private readonly ILogger _logger;
    private readonly IShellFeaturesManager _shellFeaturesManager;

    private readonly Dictionary<string, IShapeTemplateViewEngine> _viewEnginesByExtension = new(StringComparer.OrdinalIgnoreCase);
    private readonly string[] _viewEnginesFileExtensions;

    public ShapeTemplateBindingStrategy(
        IEnumerable<IShapeTemplateHarvester> harvesters,
        IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IShapeTemplateViewEngine> shapeTemplateViewEngines,
        IShapeTemplateFileProviderAccessor fileProviderAccessor,
        ILogger<ShapeTemplateBindingStrategy> logger)
    {
        _harvesters = harvesters;
        _shellFeaturesManager = shellFeaturesManager;
        _shapeTemplateViewEngines = shapeTemplateViewEngines;
        _fileProviderAccessor = fileProviderAccessor;
        _logger = logger;

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

        _viewEnginesFileExtensions = _viewEnginesByExtension.Keys.ToArray();
    }

    public bool DisableMonitoring { get; set; }

    public override async ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        _logger.LogInformation("Start discovering shapes");

        var harvesterInfos = _harvesters
            .Select(harvester => new
            {
                harvester,
                subPaths = harvester.SubPaths()
            })
            .ToList();

        var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        var enabledFeatureIds = enabledFeatures.Select(f => f.Id).ToHashSet();

        // Excludes the extensions whose templates are already associated to an excluded feature that is still enabled.
        var activeExtensions = enabledFeatures
            .GroupBy(f => f.Extension.Id)
            .Select(g => g.First().Extension)
            .Where(e => !e.Features.Any(f => builder.ExcludedFeatureIds.Contains(f.Id) && enabledFeatureIds.Contains(f.Id)));

        var hits = activeExtensions.Select(extensionDescriptor =>
        {
            _logger.LogInformation("Start discovering candidate views filenames");

            var pathContexts = harvesterInfos.SelectMany(harvesterInfo => harvesterInfo.subPaths.Select(subPath =>
            {
                var filePaths = _fileProviderAccessor.FileProvider.GetViewFilePaths(
                    PathExtensions.Combine(extensionDescriptor.SubPath, subPath),
                    _viewEnginesFileExtensions,
                    inViewsFolder: true, inDepth: false);

                return new { harvesterInfo.harvester, subPath, filePaths };
            }))
            .ToList();

            _logger.LogInformation("Done discovering candidate views filenames");

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

            return shapeContexts.Select(shapeContext => new { extensionDescriptor, shapeContext });
        }).SelectMany(hits2 => hits2);

        foreach (var hit in hits)
        {
            // The template files of an active module need to be associated to all of its enabled features.
            foreach (var feature in hit.extensionDescriptor.Features.Where(f => enabledFeatureIds.Contains(f.Id)))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Binding '{TemplatePath}' as shape '{ShapeType}' for feature '{FeatureName}'",
                        hit.shapeContext.harvestShapeInfo.RelativePath,
                        hit.shapeContext.harvestShapeHit.ShapeType,
                        feature.Id);
                }

                var relativePath = hit.shapeContext.harvestShapeInfo.RelativePath;
                var viewEngineType = _viewEnginesByExtension[hit.shapeContext.harvestShapeInfo.Extension].GetType();

                builder.Describe(hit.shapeContext.harvestShapeHit.ShapeType)
                    .From(feature)
                    .BoundAs(
                        relativePath, displayContext =>
                        {
                            var viewEngine = displayContext.ServiceProvider
                                .GetServices<IShapeTemplateViewEngine>()
                                .Single(e => e.GetType() == viewEngineType);

                            return viewEngine.RenderAsync(relativePath, displayContext);
                        });
            }
        }

        _logger.LogInformation("Done discovering shapes");
    }
}
