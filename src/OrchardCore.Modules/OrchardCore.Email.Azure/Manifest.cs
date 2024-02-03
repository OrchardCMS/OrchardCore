using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure Email",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides the configuration of email settings and a default email service utilizing Azure Communication Services (ACS).",
    Dependencies = ["OrchardCore.Email"],
    Category = "Messaging"
)]
