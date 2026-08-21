using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.DataOrchestrator.Activities;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Default implementation of <see cref="IEtlActivityDisplayManager"/> that builds
/// display and editor shapes for ETL activities.
/// </summary>
public sealed class EtlActivityDisplayManager : IEtlActivityDisplayManager
{
    private readonly DisplayManager<IEtlActivity> _displayManager;

    public EtlActivityDisplayManager(
        IOptions<EtlOptions> etlOptions,
        IServiceProvider serviceProvider,
        IShapeFactory shapeFactory,
        IEnumerable<IShapePlacementProvider> placementProviders,
        IEnumerable<IDisplayDriver<IEtlActivity>> displayDrivers,
        ILogger<DisplayManager<IEtlActivity>> displayManagerLogger,
        ILayoutAccessor layoutAccessor)
    {
        var drivers = etlOptions.Value.ActivityDisplayDriverTypes
            .Select(x => serviceProvider.CreateInstance<IDisplayDriver<IEtlActivity>>(x))
            .Concat(displayDrivers);

        _displayManager = new DisplayManager<IEtlActivity>(
            drivers,
            shapeFactory,
            placementProviders,
            displayManagerLogger,
            layoutAccessor);
    }

    /// <inheritdoc />
    public Task<IShape> BuildDisplayAsync(IEtlActivity model, IUpdateModel updater, string displayType = "", string groupId = "")
    {
        return _displayManager.BuildDisplayAsync(model, updater, displayType, groupId);
    }

    /// <inheritdoc />
    public Task<IShape> BuildEditorAsync(IEtlActivity model, IUpdateModel updater, bool isNew, string groupId = "", string htmlPrefix = "")
    {
        return _displayManager.BuildEditorAsync(model, updater, isNew, groupId, htmlPrefix);
    }

    /// <inheritdoc />
    public Task<IShape> UpdateEditorAsync(IEtlActivity model, IUpdateModel updater, bool isNew, string groupId = "", string htmlPrefix = "")
    {
        return _displayManager.UpdateEditorAsync(model, updater, isNew, groupId, htmlPrefix);
    }
}
