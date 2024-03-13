using System.Threading.Tasks;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.ViewModels;

namespace OrchardCore.Users.AuditTrail.Drivers
{
    public class AuditTrailUserEventDisplayDriver : AuditTrailEventSectionDisplayDriver<AuditTrailUserEvent>
    {
        public override Task<IDisplayResult> DisplayAsync(AuditTrailEvent auditTrailEvent, AuditTrailUserEvent userEvent, BuildDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<AuditTrailUserEventDetailViewModel>("AuditTrailUserEventDetail_DetailAdmin", m => BuildViewModel(m, auditTrailEvent, userEvent))
                    .Location("DetailAdmin", "Content:10")
            );
        }

        private static void BuildViewModel(AuditTrailUserEventViewModel m, AuditTrailEvent auditTrailEvent, AuditTrailUserEvent userEvent)
        {
            m.AuditTrailEvent = auditTrailEvent;
            m.Name = userEvent.Name;
            m.UserId = userEvent.UserId;
            m.UserName = userEvent.UserName;
        }
    }
}
