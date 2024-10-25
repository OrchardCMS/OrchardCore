using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure Communication Services Email",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides email service providers leveraging Azure Communication Services (ACS).",
    Dependencies =
    [
        "OrchardCore.Email",
    ],
    Category = "Messaging"
)]
