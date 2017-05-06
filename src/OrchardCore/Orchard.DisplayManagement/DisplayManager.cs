using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Theming;

namespace Orchard.DisplayManagement
{
    public class DisplayManager<TModel> : BaseDisplayManager, IDisplayManager<TModel>
    {
        private readonly IEnumerable<IDisplayDriver<TModel>> _drivers;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IThemeManager _themeManager;
        private readonly ILayoutAccessor _layoutAccessor;

        public DisplayManager(
            IEnumerable<IDisplayDriver<TModel>> drivers,
            IShapeTableManager shapeTableManager,
            IShapeFactory shapeFactory,
            IThemeManager themeManager,
            ILogger<DisplayManager<TModel>> logger,
            ILayoutAccessor layoutAccessor
            ) : base(shapeTableManager, shapeFactory, themeManager)
        {
            _shapeTableManager = shapeTableManager;
            _shapeFactory = shapeFactory;
            _themeManager = themeManager;
            _layoutAccessor = layoutAccessor;
            _drivers = drivers;

            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<dynamic> BuildDisplayAsync(TModel model, IUpdateModel updater, string displayType = null, string group = null)
        {
            var actualShapeType = model.GetType().Name;
            var actualDisplayType = string.IsNullOrEmpty(displayType) ? "Detail" : displayType;

            // _[DisplayType] is only added for the ones different than Detail
            if (actualDisplayType != "Detail")
            {
                actualShapeType = actualShapeType + "_" + actualDisplayType;
            }

            dynamic shape = CreateContentShape(actualShapeType);

            var context = new BuildDisplayContext(
                shape,
                actualDisplayType,
                group ?? "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async driver =>
            {
                var result = await driver.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return shape;
        }

        public async Task<dynamic> BuildEditorAsync(TModel model, IUpdateModel updater, string group = null)
        {
            dynamic shape = CreateContentShape(model.GetType().Name + "_Edit");

            var context = new BuildEditorContext(
                shape,
                group ?? "",
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async driver =>
            {
                var result = await driver.BuildEditorAsync(model, context);
                if (result != null)
                {
                    result.Apply(context);
                }
            }, Logger);

            return shape;
        }

        public async Task<dynamic> UpdateEditorAsync(TModel model, IUpdateModel updater, string group = null)
        {
            dynamic shape = CreateContentShape(model.GetType().Name + "_Edit");

            var context = new UpdateEditorContext(
                shape,
                group ?? "",
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async driver =>
            {
                var result = await driver.UpdateEditorAsync(model, context);
                if (result != null)
                {
                    result.Apply(context);
                }
            }, Logger);

            return shape;
        }
    }
}
