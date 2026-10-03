using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Name = "Secrets",
    Id = "OrchardCore.Secrets",
    Description = "Provides secure storage and management of sensitive data such as passwords, API keys, and certificates.",
    Category = "Security"
)]
