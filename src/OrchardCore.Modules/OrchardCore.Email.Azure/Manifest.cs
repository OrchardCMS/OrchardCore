using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure Email Communication Services",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides email settings configuration and a default email service based on Azure Communication Services (ACS).",
    Dependencies = ["OrchardCore.Resources"],
    Category = "Messaging"
)]
