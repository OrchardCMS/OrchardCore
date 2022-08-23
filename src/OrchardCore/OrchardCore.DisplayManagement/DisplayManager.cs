using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement
{
    public class DisplayManager<TModel> : BaseDisplayManager, IDisplayManager<TModel>
    {
        private readonly IEnumerable<IDisplayDriver<TModel>> _drivers;
        private readonly IShapeFactory _shapeFactory;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ILogger _logger;

        public DisplayManager(
            IEnumerable<IDisplayDriver<TModel>> drivers,
            IShapeFactory shapeFactory,
            IEnumerable<IShapePlacementProvider> placementProviders,
            ILogger<DisplayManager<TModel>> logger,
            ILayoutAccessor layoutAccessor
            ) : base(shapeFactory, placementProviders)
        {
            _shapeFactory = shapeFactory;
            _layoutAccessor = layoutAccessor;
            _drivers = drivers;
            _logger = logger;
        }

        public async Task<IShape> BuildDisplayAsync(TModel model, IUpdateModel updater, string displayType = null, string group = null)
        {
            var actualShapeType = typeof(TModel).Name;

            var actualDisplayType = string.IsNullOrEmpty(displayType) ? "Detail" : displayType;

            // _[DisplayType] is only added for the ones different than Detail
            if (actualDisplayType != "Detail")
            {
                actualShapeType = actualShapeType + "_" + actualDisplayType;
            }

            var shape = await CreateContentShapeAsync(actualShapeType);

            // This provides a way to default a safe default and customize for each model type
            shape.Metadata.Alternates.Add($"{actualShapeType}__{model.GetType().Name}");

            var context = new BuildDisplayContext(
                shape,
                actualDisplayType,
                group ?? "",
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                new ModelStateWrapperUpdater(updater)
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async (driver, model, context) =>
            {
                var result = await driver.BuildDisplayAsync(model, context);
                if (result != null)
                {
                    await result.ApplyAsync(context);
                }
            }, model, context, _logger);

            return shape;
        }

        public async Task<IShape> BuildEditorAsync(TModel model, IUpdateModel updater, bool isNew, string group = null, string htmlPrefix = "")
        {
            var actualShapeType = typeof(TModel).Name + "_Edit";

            var shape = await CreateContentShapeAsync(actualShapeType);

            // This provides a way to default a safe default and customize for each model type
            shape.Metadata.Alternates.Add($"{model.GetType().Name}_Edit");
            shape.Metadata.Alternates.Add($"{actualShapeType}__{model.GetType().Name}");

            var context = new BuildEditorContext(
                shape,
                group ?? "",
                isNew,
                htmlPrefix,
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                new ModelStateWrapperUpdater(updater)
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async (driver, model, context) =>
            {
                var result = await driver.BuildEditorAsync(model, context);
                if (result != null)
                {
                    await result.ApplyAsync(context);
                }
            }, model, context, _logger);

            return shape;
        }

        public async Task<IShape> UpdateEditorAsync(TModel model, IUpdateModel updater, bool isNew, string group = null, string htmlPrefix = "")
        {
            var actualShapeType = typeof(TModel).Name + "_Edit";

            var shape = await CreateContentShapeAsync(actualShapeType);

            // This provides a way to default a safe default and customize for each model type
            shape.Metadata.Alternates.Add($"{model.GetType().Name}_Edit");
            shape.Metadata.Alternates.Add($"{actualShapeType}__{model.GetType().Name}");

            var context = new UpdateEditorContext(
                shape,
                group ?? "",
                isNew,
                htmlPrefix,
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                new ModelStateWrapperUpdater(updater)
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async (driver, model, context) =>
            {
                var result = await driver.UpdateEditorAsync(model, context);
                if (result != null)
                {
                    await result.ApplyAsync(context);
                }
            }, model, context, _logger);

            return shape;
        }
    }
}
