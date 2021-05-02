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
    public class AuditTrailEventDisplayManager : IAuditTrailEventDisplayManager
    {
        private readonly IShapeFactory _shapeFactory;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IEnumerable<IAuditTrailEventDriver> _auditTrailEventDrivers;
        private readonly ILogger _logger;

        public AuditTrailEventDisplayManager(
            IShapeFactory shapeFactory,
            IAuditTrailManager auditTrailManager,
            IEnumerable<IAuditTrailEventDriver> auditTrailEventDrivers,
            ILogger<AuditTrailEventDisplayManager> logger)
        {
            _shapeFactory = shapeFactory;
            _auditTrailManager = auditTrailManager;
            _auditTrailEventDrivers = auditTrailEventDrivers;
            _logger = logger;
        }

        public async Task<IShape> BuildDisplayFiltersAsync(Filters filters)
        {
            var shape = await _shapeFactory.CreateAsync("AuditTrailFilters");
            var categories = _auditTrailManager.DescribeCategories().ToArray();

            var context = new DisplayFiltersContext(filters, categories, _shapeFactory, shape);
            await _auditTrailEventDrivers.InvokeAsync((handler, context) => handler.DisplayFiltersAsync(context), context, _logger);

            return shape;
        }

        public async Task<IShape> BuildDisplayColumnNamesAsync()
        {
            var shape = await _shapeFactory.CreateAsync("AuditTrailEventColumnNames");

            var context = new DisplayColumnNamesContext(shape);
            await _auditTrailEventDrivers.InvokeAsync((handler, context) => handler.DisplayColumnNamesAsync(context), context, _logger);

            return shape;
        }

        public async Task<IShape> BuildDisplayColumnsAsync(AuditTrailEvent auditTrailEvent)
        {
            var shape = await _shapeFactory.CreateAsync("AuditTrailEventColumns");

            var context = new DisplayColumnsContext(auditTrailEvent, shape);
            await _auditTrailEventDrivers.InvokeAsync((handler, context) => handler.DisplayColumnsAsync(context), context, _logger);

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

            var shape = await _shapeFactory.CreateAsync<AuditTrailEventViewModel>(shapeType, async viewModel =>
            {
                ((IShape)viewModel).Metadata.DisplayType = displayType;

                viewModel.AuditTrailEvent = auditTrailEvent;
                viewModel.EventDescriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);

                await _auditTrailEventDrivers.InvokeAsync((handler, model) => handler.BuildViewModelAsync(viewModel), viewModel, _logger);
            });

            shape.Metadata.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}");
            shape.Metadata.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}");
            shape.Metadata.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}__{auditTrailEvent.EventName}");
            shape.Metadata.Alternates.Add($"{shapeType}__{auditTrailEvent.Category}__{auditTrailEvent.EventName}");

            return shape;
        }
    }
}
