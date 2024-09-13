using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Name = "Azure SMS",
    Id = "OrchardCore.Sms.Azure",
    Description = "Provides settings and services to send SMS messages using Azure Communication Services (ACS).",
    Category = "SMS"
)]
