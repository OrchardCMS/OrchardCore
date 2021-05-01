using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailEventDisplayManager : IAuditTrailEventDisplayManager
    {
        private readonly ISession _session;
        private readonly IShapeFactory _shapeFactory;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IEnumerable<IAuditTrailEventHandler> _auditTrailEventHandlers;
        private readonly ILogger _logger;

        public AuditTrailEventDisplayManager(
            ISession session,
            IShapeFactory shapeFactory,
            IAuditTrailManager auditTrailManager,
            ILogger<AuditTrailEventDisplayManager> logger,
            IEnumerable<IAuditTrailEventHandler> auditTrailEventHandlers)
        {
            _session = session;
            _shapeFactory = shapeFactory;
            _auditTrailManager = auditTrailManager;
            _auditTrailEventHandlers = auditTrailEventHandlers;
            _logger = logger;
        }

        public async Task<IShape> BuildFilterDisplayAsync(Filters filters)
        {
            var filterDisplay = await _shapeFactory.CreateAsync("AuditTrailFilter");
            var filterDisplayContext = new DisplayFilterContext(_shapeFactory, filters, filterDisplay as Shape);

            await _auditTrailEventHandlers.InvokeAsync((handler, context) => handler.DisplayFilterAsync(context), filterDisplayContext, _logger);

            // Give each provider a chance to provide a filter display.
            var providersContext = _auditTrailManager.DescribeProviders();
            foreach (var action in providersContext.FilterDisplays)
            {
                await action(filterDisplayContext);
            }

            return filterDisplay;
        }

        public async Task<IShape> BuildAdditionalColumnNamesShapesAsync()
        {
            var additionalColumnNamesDisplay = await _shapeFactory.CreateAsync("AuditTrailEventAdditionalColumnName");

            await _auditTrailEventHandlers.InvokeAsync((handler, display) => handler.DisplayAdditionalColumnNamesAsync(display), additionalColumnNamesDisplay as Shape, _logger);

            return additionalColumnNamesDisplay;
        }

        public async Task<IShape> BuildAdditionalColumnsShapesAsync(AuditTrailEvent auditTrailEvent)
        {
            var additionalColumnDisplay = await _shapeFactory.CreateAsync("AuditTrailEventAdditionalColumn");

            await _auditTrailEventHandlers.InvokeAsync(
                (handler, display) =>
                handler.DisplayAdditionalColumnsAsync(display),
                new DisplayAdditionalColumnsContext(auditTrailEvent, additionalColumnDisplay as Shape),
                _logger);

            return additionalColumnDisplay;
        }

        public Task<IShape> BuildDisplayAsync(AuditTrailEvent auditTrailEvent, string displayType) =>
            BuildEventShapeAsync("AuditTrailEvent", auditTrailEvent, displayType);

        public Task<IShape> BuildActionsAsync(AuditTrailEvent auditTrailEvent, string displayType) =>
            BuildEventShapeAsync("AuditTrailEventActions", auditTrailEvent, displayType);

        private async Task<IShape> BuildEventShapeAsync(string shapeType, AuditTrailEvent auditTrailEvent, string displayType)
        {
            var auditTrailEventActionsShape = await _shapeFactory.CreateAsync<AuditTrailEventViewModel>(shapeType, async model =>
            {
                model.AuditTrailEvent = auditTrailEvent;
                model.EventDescriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);

                var metaData = ((IShape)model).Metadata;
                metaData.DisplayType = displayType;

                metaData.Alternates.Add($"{shapeType}_{displayType}");
                metaData.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}");
                metaData.Alternates.Add($"{shapeType}_{displayType}__{auditTrailEvent.Category}");
                metaData.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}__{auditTrailEvent.EventName}");
                metaData.Alternates.Add($"{shapeType}_{displayType}__{auditTrailEvent.Category}__{auditTrailEvent.EventName}");

                await _auditTrailEventHandlers.InvokeAsync((handler, model) => handler.BuildViewModelAsync(model), model, _logger);
            });

            return auditTrailEventActionsShape;
        }
    }
}
