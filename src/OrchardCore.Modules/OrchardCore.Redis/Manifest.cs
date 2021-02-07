using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Redis",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Redis",
    Name = "Redis",
    Description = "Redis configuration support.",
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Redis.Cache",
    Name = "Redis Cache",
    Description = "Distributed cache using Redis.",
    Dependencies = new[] { "OrchardCore.Redis" },
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Redis.Bus",
    Name = "Redis Bus",
    Description = "Makes the Signal service distributed.",
    Dependencies = new[] { "OrchardCore.Redis" },
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Redis.Lock",
    Name = "Redis Lock",
    Description = "Distributed Lock using Redis.",
    Dependencies = new[] { "OrchardCore.Redis" },
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Redis.DataProtection",
    Name = "Redis DataProtection",
    Description = "Distributed DataProtection using Redis.",
    Dependencies = new[] { "OrchardCore.Redis" },
    Category = "Distributed"
)]
