using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications",
    Name = "Notifications Core Services",
    Description = "Provides notification core services.",
    Category = "Notifications",
    EnabledByDependencyOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications.Web",
    Name = "Web Notifications",
    Description = "Provides a way to sent Web notifications to users.",
    Category = "Notifications",
    Dependencies = new[]
    {
        "OrchardCore.Notifications"
    }
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications.Email",
    Name = "Email Notifications",
    Description = "Provides a way to sent Email notifications to users.",
    Category = "Notifications",
    Dependencies = new[]
    {
        "OrchardCore.Notifications",
        "OrchardCore.Email"
    }
)]
