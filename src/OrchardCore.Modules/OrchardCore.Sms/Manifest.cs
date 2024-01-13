using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Name = "SMS",
    Id = "OrchardCore.Sms",
    Description = "Provides settings and services to send SMS messages.",
    Category = "SMS"
)]

[assembly: Feature(
    Name = "Twilio SMS Provider",
    Id = "OrchardCore.Sms.Twilio",
    Description = "Provides Twilio SMS services for sending messages to users.",
    Category = "SMS",
    Dependencies =
    [
        "OrchardCore.Sms",
    ]
)]

[assembly: Feature(
    Name = "SMS Notifications",
    Id = "OrchardCore.Notifications.Sms",
    Description = "Provides a way to send SMS notifications to users.",
    Category = "Notifications",
    Dependencies =
    [
        "OrchardCore.Notifications",
        "OrchardCore.Sms",
    ]
)]
