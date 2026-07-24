using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Rate Limits",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.RateLimits",
    Name = "Rate Limits",
    Description = "Provides a way to manage rate limiting to the website.",
    Category = "Security",
    Priority = "-100" // Ensure that the Rate Limits feature is loaded before other features that may depend on it.
)]
