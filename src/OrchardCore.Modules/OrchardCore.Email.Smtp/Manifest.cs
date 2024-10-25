using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "SMTP Email Provider",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides an email service provider leveraging Simple Mail Transfer Protocol (SMTP).",
    Dependencies =
    [
        "OrchardCore.Email"
    ],
    Category = "Messaging"
)]
