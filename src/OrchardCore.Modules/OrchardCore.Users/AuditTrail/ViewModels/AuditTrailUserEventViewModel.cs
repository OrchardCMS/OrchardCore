using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.Models;

namespace OrchardCore.Users.AuditTrail.ViewModels;

public class AuditTrailUserEventViewModel
{
    [BindNever]
    public AuditTrailUserEvent UserEvent { get; set; }

    [BindNever]
    public AuditTrailEvent AuditTrailEvent { get; set; }
}
