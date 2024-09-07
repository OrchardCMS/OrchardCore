namespace OrchardCore.AuditTrail.Settings;

public class AuditTrailSettings
{
    public AuditTrailCategorySettings[] Categories { get; set; } = [];
    public bool ClientIpAddressAllowed { get; set; }
}
