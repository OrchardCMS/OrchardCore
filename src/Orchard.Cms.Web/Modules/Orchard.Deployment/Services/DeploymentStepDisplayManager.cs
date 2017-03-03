using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.Deployment.Editors;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Theming;

namespace Orchard.Deployment.Services
{
    public class DeploymentStepDisplayManager : BaseDisplayManager, IDeploymentStepDisplayManager
    {
        private readonly IEnumerable<IDeploymentStepDisplayDriver> _drivers;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IThemeManager _themeManager;
        private readonly ILayoutAccessor _layoutAccessor;

        public DeploymentStepDisplayManager(
            IEnumerable<IDeploymentStepDisplayDriver> drivers,
            IShapeTableManager shapeTableManager,
            IShapeFactory shapeFactory,
            IThemeManager themeManager,
            ILogger<DeploymentStepDisplayManager> logger,
            ILayoutAccessor layoutAccessor
            ) : base(shapeTableManager, shapeFactory, themeManager)
        {
            _drivers = drivers;
            _shapeTableManager = shapeTableManager;
            _shapeFactory = shapeFactory;
            _themeManager = themeManager;
            _layoutAccessor = layoutAccessor;

            Logger = logger;
        }

        public ILogger Logger;

        public async Task<dynamic> DisplayStepAsync(DeploymentStep step, IUpdateModel updater, string displayType = null)
        {
            var actualShapeType = "DeploymentStep";
            var actualDisplayType = string.IsNullOrEmpty(displayType) ? "Summary" : displayType;

            // _[DisplayType] is only added for the ones different than Detail
            if (actualDisplayType != "Summary")
            {
                actualShapeType = actualShapeType + "_" + actualDisplayType;
            }

            dynamic deploymentStepShape = CreateContentShape(actualShapeType);
            deploymentStepShape.DeploymentStep = step;

            var context = new BuildDisplayContext(
                deploymentStepShape,
                actualDisplayType,
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async driver =>
            {
                var result = await driver.BuildDisplayAsync(step, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return deploymentStepShape;
        }

        public async Task<dynamic> BuildStepEditorAsync(DeploymentStep step, IUpdateModel updater)
        {
            dynamic deploymentStepShape = CreateContentShape("DeploymentStep_Edit");
            deploymentStepShape.DeploymentStep = step;

            var context = new BuildEditorContext(
                deploymentStepShape,
                "",
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async driver =>
            {
                var result = await driver.BuildEditorAsync(step, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return deploymentStepShape;
        }

        public async Task<dynamic> UpdatStepEditorAsync(DeploymentStep step, IUpdateModel updater)
        {
            dynamic deploymentStepShape = CreateContentShape("DeploymentStep_Edit");
            deploymentStepShape.DeploymentStep = step;

            var context = new UpdateEditorContext(
                deploymentStepShape,
                "",
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _drivers.InvokeAsync(async driver =>
            {
                var result = await driver.UpdateEditorAsync(step, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return deploymentStepShape;
        }
    }
}
