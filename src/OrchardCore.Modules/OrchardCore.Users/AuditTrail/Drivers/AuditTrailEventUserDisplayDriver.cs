using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.ViewModels;

namespace OrchardCore.Users.AuditTrail.Drivers
{
    public class AuditTrailUserEventDisplayDriver : SectionDisplayDriver<AuditTrailEvent, AuditTrailUserEvent>
    {
        public override Task<IDisplayResult> DisplayAsync(AuditTrailEvent auditTrailEvent, AuditTrailUserEvent userEvent, BuildDisplayContext context)
        {
            if (!auditTrailEvent.Properties.ContainsKey(PropertyName))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            return Task.FromResult<IDisplayResult>(
                Combine(
                    Initialize<AuditTrailUserEventViewModel>("AuditTrailUserEventHeader_SummaryAdmin", m => BuildViewModel(m, auditTrailEvent, userEvent))
                        .Location("SummaryAdmin", "Header:10"),
                    Initialize<AuditTrailUserEventDetailViewModel>("AuditTrailUserEventDetail_DetailAdmin", m =>
                    {
                        BuildViewModel(m, auditTrailEvent, userEvent);
                    }).Location("DetailAdmin", "Content:10")
            ));
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
