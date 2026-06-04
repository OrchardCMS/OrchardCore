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
    Description = "Provides configurable global and route-based rate limiting.",
    Category = "Security"
)]
