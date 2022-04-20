using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailItemViewModel
    {
        [BindNever]
        public IShape Shape { get; set; }
    }
}
