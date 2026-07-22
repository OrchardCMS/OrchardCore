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
    After =
    [
        "OrchardCore.Media",
        "OrchardCore.Tenants.FileProvider"
    ],
    Category = "Security"
)]
