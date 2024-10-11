using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Email",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides the necessary infrastructure for configuring email settings. Email providers can be enabled in separate features.",
    Category = "Messaging"
)]
