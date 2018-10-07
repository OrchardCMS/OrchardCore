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
    Description = "Core components using Message bus.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis",
    Name = "Redis",
    Description = "Configuration to connect to Redis.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Cache",
    Name = "Redis Cache",
    Description = "Distributed cache implementation using Redis.",
    Dependencies = new[] { "OrchardCore.Distributed.Redis" },
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.MessageBus",
    Name = "Redis Message Bus",
    Description = "Message bus implementation using Redis.",
    Dependencies = new[] { "OrchardCore.Distributed", "OrchardCore.Distributed.Redis" },
    Category = "Hosting"
)]
