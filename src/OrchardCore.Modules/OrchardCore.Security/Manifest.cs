using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Security",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The security module adds the required security headers for the best practices.",
    Category = "Security",
    IsAlwaysEnabled = true
)]
