using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications",
    Name = "Notifications",
    Description = "Provides a way to notify users.",
    Category = "Notifications"
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications.Email",
    Name = "Email Notifications",
    Description = "Provides a way to sent email notifications to users.",
    Category = "Notifications",
    Dependencies = new[]
    {
        "OrchardCore.Notifications",
        "OrchardCore.Email",
    }
)]
