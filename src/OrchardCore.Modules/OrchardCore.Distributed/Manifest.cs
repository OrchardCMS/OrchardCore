using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Distributed Services",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed",
    Name = "Distributed services",
    Description = "Some Default implementations of distributed services.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Settings",
    Name = "Distributed Redis Settings",
    Description = "Configuration to connect to Redis server(s).",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Cache",
    Name = "Distributed Redis Cache",
    Description = "Distributed Cache implementation using Redis.",
    Dependencies = new[] { "OrchardCore.Distributed.Redis.Settings" },
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.MessageBus",
    Name = "Distributed Redis MessageBus",
    Description = "Distributed Signal implementation using Redis.",
    Dependencies = new[] { "OrchardCore.Distributed", "OrchardCore.Distributed.Redis.Settings" },
    Category = "Hosting"
)]
