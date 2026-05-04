namespace OrchardCore.Users.AuditTrail.ViewModels;

public class AuditTrailUserEventSettingsViewModel
{
    public UserSnapshotPropertiesEntry[] UserSnapshotProperties { get; set; } = [];

    public class UserSnapshotPropertiesEntry
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
    };
}
