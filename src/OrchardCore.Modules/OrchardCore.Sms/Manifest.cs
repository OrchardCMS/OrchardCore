using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Name = "SMS",
    Id = "OrchardCore.Sms",
    Description = "Providers settings and services to send SMS messages.",
    Category = "SMS"
)]

[assembly: Feature(
    Name = "SMS Notifications",
    Id = "OrchardCore.Notifications.Sms",
    Description = "Provides a way to sent SMS notifications to users.",
    Category = "Notifications",
    Dependencies = new[]
    {
        "OrchardCore.Notifications",
        "OrchardCore.Sms",
    }
)]
