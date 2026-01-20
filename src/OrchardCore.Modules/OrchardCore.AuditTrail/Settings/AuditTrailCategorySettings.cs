namespace OrchardCore.AuditTrail.Settings;

public class AuditTrailCategorySettings
{
    public string Name { get; set; }
    public AuditTrailEventSettings[] Events { get; set; } = [];
}
