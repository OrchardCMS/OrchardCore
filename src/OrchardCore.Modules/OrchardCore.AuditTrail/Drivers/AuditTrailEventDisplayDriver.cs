using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AuditTrail.Drivers
{
    public class AuditTrailEventDisplayDriver : DisplayDriver<AuditTrailEvent>
    {
        private readonly IAuditTrailManager _auditTrailManager;

        public AuditTrailEventDisplayDriver(
            IAuditTrailManager auditTrailManager)
        {
            _auditTrailManager = auditTrailManager;
        }

        public override IDisplayResult Display(AuditTrailEvent auditTrailEvent)
        {
            // TODO BuildViewModel
            //describe event once.
            return Combine(
                Initialize<AuditTrailEventViewModel>("AuditTrailEventTags_SummaryAdmin", model =>
                {
                    model.AuditTrailEvent = auditTrailEvent;
                    model.Descriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);
                }).Location("SummaryAdmin", "EventTags:10"),
                Initialize<AuditTrailEventViewModel>("AuditTrailEventMeta_SummaryAdmin", model =>
                {
                    model.AuditTrailEvent = auditTrailEvent;
                    model.Descriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);
                }).Location("SummaryAdmin", "EventMeta:10"),
                Initialize<AuditTrailEventViewModel>("AuditTrailEventActions_SummaryAdmin", model =>
                {
                    model.AuditTrailEvent = auditTrailEvent;
                    model.Descriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);
                }).Location("SummaryAdmin", "Actions:10"),
                Initialize<AuditTrailEventViewModel>("AuditTrailEventDetail_DetailAdmin", model =>
                {
                    model.AuditTrailEvent = auditTrailEvent;
                    model.Descriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);
                }).Location("DetailAdmin", "Content:before")
            );
        }

    }
}
