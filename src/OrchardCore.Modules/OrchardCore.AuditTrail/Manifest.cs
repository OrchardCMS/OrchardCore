using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Audit Trail",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.AuditTrail",
    Name = "Audit Trail",
    Description = "Provides a log for recording and viewing back-end changes.",
    Category = "Security"
)]
