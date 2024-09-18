using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Name = "Azure Communication Services SMS",
    Id = "OrchardCore.Sms.Azure",
    Description = "Enables the ability to send SMS messages through Azure Communication Services (ACS).",
    Dependencies =
    [
        "OrchardCore.Sms",
    ],
    Category = "SMS"
)]
