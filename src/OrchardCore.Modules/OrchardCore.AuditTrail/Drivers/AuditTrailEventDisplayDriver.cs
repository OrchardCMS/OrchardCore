using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AuditTrail.Drivers
{
    public class AuditTrailEventDisplayDriver : DisplayDriver<AuditTrailEvent>
    {
        private readonly IAuditTrailManager _auditTrailManager;

        public AuditTrailEventDisplayDriver(IAuditTrailManager auditTrailManager)
        {
            _auditTrailManager = auditTrailManager;
        }

        public override IDisplayResult Display(AuditTrailEvent auditTrailEvent)
        {
            var descriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);

            return Combine(
                Initialize<AuditTrailEventViewModel>("AuditTrailEventTags_SummaryAdmin", model => BuildViewModel(auditTrailEvent, model, descriptor))
                    .Location("SummaryAdmin", "EventTags:10"),
                Initialize<AuditTrailEventViewModel>("AuditTrailEventMeta_SummaryAdmin", model => BuildViewModel(auditTrailEvent, model, descriptor))
                    .Location("SummaryAdmin", "EventMeta:10"),
                Initialize<AuditTrailEventViewModel>("AuditTrailEventActions_SummaryAdmin", model => BuildViewModel(auditTrailEvent, model, descriptor))
                    .Location("SummaryAdmin", "Actions:10"),
                Initialize<AuditTrailEventViewModel>("AuditTrailEventDetail_DetailAdmin", model => BuildViewModel(auditTrailEvent, model, descriptor))
                    .Location("DetailAdmin", "Content:before")
            );
        }

        public static void BuildViewModel(AuditTrailEvent auditTrailEvent, AuditTrailEventViewModel model, AuditTrailEventDescriptor descriptor)
        {
            model.AuditTrailEvent = auditTrailEvent;
            model.Descriptor = descriptor;
        }
    }
}
