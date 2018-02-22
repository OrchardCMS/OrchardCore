using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OpenID (Entity Framework Core stores)",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "This package provides an Entity Framework Core 2.x adapter for the OpenID module.",
    Dependencies = "OrchardCore.OpenId",
    Category = "Security"
)]
