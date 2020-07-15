using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;
using IYesSqlSession = YesSql.ISession;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailEventDisplayManager : IAuditTrailEventDisplayManager
    {
        private readonly IYesSqlSession _session;
        private readonly IShapeFactory _shapeFactory;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IEnumerable<IAuditTrailEventHandler> _auditTrailEventHandlers;

        public ILogger Logger { get; set; }


        public AuditTrailEventDisplayManager(
            IYesSqlSession session,
            IShapeFactory shapeFactory,
            IAuditTrailManager auditTrailManager,
            ILogger<AuditTrailEventDisplayManager> logger,
            IEnumerable<IAuditTrailEventHandler> auditTrailEventHandlers)
        {
            _session = session;
            _shapeFactory = shapeFactory;
            _auditTrailManager = auditTrailManager;
            _auditTrailEventHandlers = auditTrailEventHandlers;

            Logger = logger;
        }


        public async Task<IShape> BuildFilterDisplayAsync(Filters filters)
        {
            var filterDisplay = await _shapeFactory.CreateAsync("AuditTrailFilter");
            var filterDisplayContext = new DisplayFilterContext(_shapeFactory, filters, filterDisplay as Shape);

            _auditTrailEventHandlers.Invoke((handler, context) => handler.DisplayFilterAsync(context), filterDisplayContext, Logger);

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

            _auditTrailEventHandlers.Invoke((handler, display) => handler.DisplayAdditionalColumnNamesAsync(display), additionalColumnNamesDisplay as Shape, Logger);

            return additionalColumnNamesDisplay;
        }

        public async Task<IShape> BuildAdditionalColumnsShapesAsync(AuditTrailEvent auditTrailEvent)
        {
            var additionalColumnDisplay = await _shapeFactory.CreateAsync("AuditTrailEventAdditionalColumn");

            _auditTrailEventHandlers.Invoke((handler, display) => handler.DisplayAdditionalColumnsAsync(display),
                new DisplayAdditionalColumnsContext(auditTrailEvent, additionalColumnDisplay as Shape), Logger);

            return additionalColumnDisplay;
        }

        public async Task<IShape> BuildDisplayAsync(AuditTrailEvent auditTrailEvent, string displayType) =>
            await BuildEventShapeAsync("AuditTrailEvent", auditTrailEvent, displayType);

        public async Task<IShape> BuildActionsAsync(AuditTrailEvent auditTrailEvent, string displayType) =>
            await BuildEventShapeAsync("AuditTrailEventActions", auditTrailEvent, displayType);


        private async Task<IShape> BuildEventShapeAsync(string shapeType, AuditTrailEvent auditTrailEvent, string displayType)
        {
            dynamic auditTrailEventActionsShape = await _shapeFactory.CreateAsync(shapeType, Arguments.From(new Dictionary<string, object>
            {
                { "AuditTrailEvent", auditTrailEvent },
                { "Descriptor", _auditTrailManager.DescribeEvent(auditTrailEvent)},
                { "EventData", auditTrailEvent.Get(auditTrailEvent.EventName) }
            }));

            if (auditTrailEvent.Category == "Content")
            {
                var contentItem = auditTrailEvent.Get(auditTrailEvent.EventName).ToObject<ContentItem>();

                var availableVersionsCount = await _session.Query<ContentItem, ContentItemIndex>()
                    .Where(index => index.ContentItemId == contentItem.ContentItemId).CountAsync();

                auditTrailEventActionsShape.AvailableVersionsCount = availableVersionsCount;
            }

            var metaData = auditTrailEventActionsShape.Metadata;
            metaData.DisplayType = displayType;
            metaData.Alternates.Add($"{shapeType}_{displayType}");
            metaData.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}");
            metaData.Alternates.Add($"{shapeType}_{displayType}__{auditTrailEvent.Category}");
            metaData.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}__{auditTrailEvent.EventName}");
            metaData.Alternates.Add($"{shapeType}_{displayType}__{auditTrailEvent.Category}__{auditTrailEvent.EventName}");

            return auditTrailEventActionsShape;
        }
    }
}
