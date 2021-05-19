using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailItemViewModel
    {
        public AuditTrailEvent Event { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public LocalizedString LocalizedCategory { get; set; }
        public IShape ActionsShape { get; set; }
        public IShape EventShape { get; set; }
    }
}
