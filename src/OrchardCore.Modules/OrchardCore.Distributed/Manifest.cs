using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Distributed Services",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Signal",
    Name = "Distributed Signal",
    Description = "Distributed Signal using a Message bus.",
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Shell",
    Name = "Distributed Shell",
    Description = "Distributed Shell using a Message Bus.",
    DefaultTenantOnly = true,
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis",
    Name = "Redis",
    Description = "Allows to configure and connect to Redis.",
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Cache",
    Name = "Redis Cache",
    Description = "Distributed cache using Redis.",
    Dependencies = new[] { "OrchardCore.Distributed.Redis" },
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Bus",
    Name = "Redis Bus",
    Description = "Message bus using Redis.",
    Dependencies = new[] { "OrchardCore.Distributed.Redis" },
    Category = "Distributed"
)]

[assembly: Feature(
    Id = "OrchardCore.Distributed.Redis.Lock",
    Name = "Redis Lock",
    Description = "Distributed Lock using Redis.",
    Dependencies = new[] { "OrchardCore.Distributed.Redis" },
    Category = "Distributed"
)]
