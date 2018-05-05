using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Users (Entity Framework Core stores)",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The Users Entity Framework Core module allows to store user data via Entity Framework Core ORM",
    Dependencies = new [] { "OrchardCore.DataProtection", "OrchardCore.Resources" },
    Category = "Security"
)]
