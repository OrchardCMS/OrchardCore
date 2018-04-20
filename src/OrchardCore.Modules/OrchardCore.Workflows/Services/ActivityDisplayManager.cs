using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Workflows.Services
{
    public class ActivityDisplayManager : IActivityDisplayManager
    {
        private readonly DisplayManager<IActivity> _displayManager;
        public ActivityDisplayManager(IOptions<WorkflowOptions> workflowOptions, IServiceProvider serviceProvider, IShapeTableManager shapeTableManager, IShapeFactory shapeFactory, IThemeManager themeManager, ILogger<DisplayManager<IActivity>> displayManagerLogger, ILayoutAccessor layoutAccessor)
        {
            var drivers = workflowOptions.Value.ActivityDisplayDriverTypes.Select(x => serviceProvider.CreateInstance<IDisplayDriver<IActivity>>(x));
            _displayManager = new DisplayManager<IActivity>(drivers, shapeTableManager, shapeFactory, themeManager, displayManagerLogger, layoutAccessor);
        }

        public Task<IShape> BuildDisplayAsync(IActivity model, IUpdateModel updater, string displayType = "", string groupId = "")
        {
            return _displayManager.BuildDisplayAsync(model, updater, displayType, groupId);
        }

        public Task<IShape> BuildEditorAsync(IActivity model, IUpdateModel updater, bool isNew, string groupId = "")
        {
            return _displayManager.BuildEditorAsync(model, updater, isNew, groupId);
        }

        public Task<IShape> UpdateEditorAsync(IActivity model, IUpdateModel updater, bool isNew, string groupId = "")
        {
            return _displayManager.UpdateEditorAsync(model, updater, isNew, groupId);
        }
    }
}