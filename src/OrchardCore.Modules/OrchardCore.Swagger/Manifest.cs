using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0")]

[assembly: Feature(
    Id = "OrchardCore.Swagger",
    Name = "Swagger API",
    Category = "Swagger",
    Description = "Enables Swagger API documentation."
)]

[assembly: Feature(
    Id = "OrchardCore.Swagger.API",
    Name = "OrchardCore Swagger API documentation",
    Category = "Swagger",
    Description = "Enables the Swagger API documentation for OrchardCore API's.",
    Dependencies = new[]{"OrchardCore.Swagger"}
)]