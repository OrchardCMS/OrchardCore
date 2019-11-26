using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0")]

[assembly: Feature(
    Id = "OrchardCore.OpenApi",
    Name = "OrchardCore OpenApi documentation generation",
    Category = "OpenApi",
    Description = "Enables OpenApi documentation generation of the OrchardCore APIs."
)]