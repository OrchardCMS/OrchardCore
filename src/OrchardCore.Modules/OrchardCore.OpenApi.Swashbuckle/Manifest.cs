using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0")]

[assembly: Feature(
    Id = "OrchardCore.OpenApi.Swagger",
    Name = "Swagger OpenApi documentation",
    Category = "OpenApi",
    Description = "Enables the Swagger endpoint for displaying OpenApi documentation."
)]