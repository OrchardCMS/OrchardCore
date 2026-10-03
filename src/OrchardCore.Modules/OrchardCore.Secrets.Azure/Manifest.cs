using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Name = "Secrets - Azure Key Vault",
    Id = "OrchardCore.Secrets.Azure",
    Description = "Provides Azure Key Vault integration for storing and retrieving secrets.",
    Category = "Security",
    Dependencies = ["OrchardCore.Secrets"]
)]
