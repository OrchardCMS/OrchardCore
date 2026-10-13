using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListColumnsBuilder : IAdminListColumnsBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ILogger _logger;

    public DefaultAdminListColumnsBuilder(
        IServiceProvider serviceProvider,
        IShapeFactory shapeFactory,
        ILayoutAccessor layoutAccessor,
        IUpdateModelAccessor updateModelAccessor,
        ILogger<DefaultAdminListColumnsBuilder> logger)
    {
        _serviceProvider = serviceProvider;
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _updateModelAccessor = updateModelAccessor;
        _logger = logger;
    }

    public async Task<AdminListColumnsContext> BuildColumnsAsync<TModel>(string listName, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        var context = new AdminListColumnsContext(
            listName,
            data,
            CreateZoneHolding(),
            _shapeFactory,
            await _layoutAccessor.GetLayoutAsync(),
            _updateModelAccessor.ModelUpdater);

        // The drivers of the items of the list are the only ones created, in the order their features depend on
        // each other: the module owning the list adds its columns before the modules depending on it change or
        // remove them. A driver adding a column the list already has throws, which is logged, and the columns it
        // added before are kept.
        await _serviceProvider.GetServices<IAdminListColumnDriver<TModel>>().InvokeAsync(
            static async (driver, context) =>
            {
                var result = await driver.BuildColumnsAsync(context);

                if (result != null)
                {
                    await result.ApplyAsync(context);
                }
            },
            context,
            _logger);

        // Sorted in place: the collection of the drivers is the one the list renders.
        context.Columns.SortByPosition();

        return context;
    }

    public async Task<IList<IZoneHolding>> BuildCellsAsync<TModel>(AdminListColumnsContext columns, IEnumerable<AdminListRow<TModel>> rows, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);

        // Resolved once for all the rows, as an array: it is enumerated for every row.
        var drivers = _serviceProvider.GetServices<IAdminListColumnDriver<TModel>>().ToArray();
        var cells = new List<IZoneHolding>();

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rowCells = CreateZoneHolding();

            if (drivers.Length > 0)
            {
                var context = new AdminListCellsContext(columns, rowCells, row.Shape);

                await drivers.InvokeAsync(
                    static async (driver, model, context) =>
                    {
                        var result = await driver.BuildCellsAsync(model, context);

                        if (result != null)
                        {
                            await result.ApplyAsync(context);
                        }
                    },
                    row.Model,
                    context,
                    _logger);
            }

            cells.Add(rowCells);
        }

        return cells;
    }

    // The headers and the cells are never rendered themselves, only their zones are, so they are not created
    // through the shape factory. Their zones are content zones, like the ones of the row shapes.
    private ZoneHolding<IShapeFactory> CreateZoneHolding()
        => new ZoneHolding<IShapeFactory>(static factory => factory.CreateAsync("ContentZone"), _shapeFactory);
}
