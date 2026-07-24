namespace OrchardCore.Users.AuditTrail.ViewModels;

public class AuditTrailUserEventSettingsViewModel
{
    public UserSnapshotPropertiesEntry[] UserSnapshotProperties { get; set; } = [];
    public IEnumerable<string> RedactorNames { get; set; }

    public class UserSnapshotPropertiesEntry
    {
        public string Name { get; set; }
        public string Redactor { get; set; }
    };
}
