using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Distributed Services",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Settings",
    Name = "Redis Settings",
    Description = "Configuration to connect to Redis server(s).",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Cache",
    Name = "Redis Cache",
    Description = "Distributed cache implementation using Redis.",
    Dependencies = new[] { "OrchardCore.Distributed.Redis.Settings" },
    Category = "Hosting"
)]
