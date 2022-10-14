using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications",
    Name = "Notifications Infrastructure",
    Description = "Provides notification infrastructure.",
    Category = "Notifications",
    EnabledByDependencyOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications.Email",
    Name = "Email Notifications",
    Description = "Provides a way to sent Email Notifications to users.",
    Category = "Notifications",
    Dependencies = new[]
    {
        "OrchardCore.Notifications",
        "OrchardCore.Email"
    }
)]
