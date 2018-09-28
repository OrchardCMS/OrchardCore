using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Distributed Services",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Cache",
    Name = "Distributed Redis Cache",
    Description = "Distributed cache implementation using Redis.",
    Category = "Hosting"
)]
