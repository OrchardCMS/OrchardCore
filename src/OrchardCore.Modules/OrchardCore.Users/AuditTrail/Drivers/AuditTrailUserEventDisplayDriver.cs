using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.ViewModels;

namespace OrchardCore.Users.AuditTrail.Drivers;

public sealed class AuditTrailUserEventDisplayDriver : AuditTrailEventSectionDisplayDriver<AuditTrailUserEvent>
{
    public override IDisplayResult Display(AuditTrailEvent auditTrailEvent, AuditTrailUserEvent userEvent, BuildDisplayContext context)
    {
        return Initialize<AuditTrailUserEventViewModel>("AuditTrailUserEventDetail_DetailAdmin", m =>
        {
            m.AuditTrailEvent = auditTrailEvent;
            m.UserEvent = userEvent;
        }).Location("DetailAdmin", "Content:10");
    }
}
