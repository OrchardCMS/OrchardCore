using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure Email Provider",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides an email service provider leveraging Azure Communication Services (ACS).",
    Dependencies =
    [
        "OrchardCore.Email"
    ],
    Category = "Messaging"
)]
