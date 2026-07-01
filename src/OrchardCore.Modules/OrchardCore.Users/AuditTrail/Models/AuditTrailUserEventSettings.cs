using Microsoft.Extensions.Compliance.Redaction;
using OrchardCore.Entities;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.AuditTrail.Models;

public class AuditTrailUserEventSettings
{
    /// <summary>
    /// Gets or sets a dictionary where the keys are properties in <see cref="User"/> or in <see
    /// cref="Entity.Properties"/>, and the values are the name of <see cref="Redactor"/> instances that are registered
    /// in the <see cref="IServiceProvider"/>.
    /// </summary>
    public Dictionary<string, string> UserSnapshotRedactors { get; set; }
}
