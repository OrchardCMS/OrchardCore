namespace OrchardCore.AuditTrail.Services
{
    public interface IAuditTrailEventIdGenerator
    {
        string GenerateUniqueId();
    }
}
