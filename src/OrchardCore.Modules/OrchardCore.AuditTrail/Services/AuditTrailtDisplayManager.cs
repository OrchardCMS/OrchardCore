using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.Modules;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailtDisplayManager : IAuditTrailDisplayManager
    {
        private readonly IShapeFactory _shapeFactory;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IEnumerable<IAuditTrailDisplayHandler> _auditTrailDisplayHandler;
        private readonly ILogger _logger;

        public AuditTrailtDisplayManager(
            IShapeFactory shapeFactory,
            IAuditTrailManager auditTrailManager,
            IEnumerable<IAuditTrailDisplayHandler> auditTrailDisplayHandler,
            ILogger<AuditTrailtDisplayManager> logger)
        {
            _shapeFactory = shapeFactory;
            _auditTrailManager = auditTrailManager;
            _auditTrailDisplayHandler = auditTrailDisplayHandler;
            _logger = logger;
        }

        public async Task<IShape> BuildDisplayFiltersAsync(Filters filters)
        {
            var shape = await _shapeFactory.CreateAsync("AuditTrailFilters");
            var categories = _auditTrailManager.DescribeCategories().ToArray();

            var context = new DisplayFiltersContext(filters, categories, _shapeFactory, shape);
            await _auditTrailDisplayHandler.InvokeAsync((handler, context) => handler.DisplayFiltersAsync(context), context, _logger);

            return shape;
        }

        public Task<IShape> BuildDisplayAsync(AuditTrailEvent auditTrailEvent, string displayType) =>
            BuildDisplayAsync("AuditTrailEvent", auditTrailEvent, displayType);

        public Task<IShape> BuildDisplayActionsAsync(AuditTrailEvent auditTrailEvent, string displayType) =>
            BuildDisplayAsync("AuditTrailEventActions", auditTrailEvent, displayType);

        private async Task<IShape> BuildDisplayAsync(string shapeType, AuditTrailEvent auditTrailEvent, string displayType)
        {
            displayType = String.IsNullOrEmpty(displayType) ? "Detail" : displayType;
            shapeType = displayType != "Detail" ? shapeType + "_" + displayType : shapeType;

            var shape = await _shapeFactory.CreateAsync<AuditTrailEventViewModel>(shapeType, model =>
            {
                model.AuditTrailEvent = auditTrailEvent;
                model.EventDescriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);
            });

            shape.Metadata.DisplayType = displayType;
            shape.Metadata.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}");
            shape.Metadata.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}__{auditTrailEvent.EventName}");

            var context = new DisplayEventContext(shape);
            await _auditTrailDisplayHandler.InvokeAsync((handler, context) => handler.DisplayEventAsync(context), context, _logger);

            return shape;
        }
    }
}
