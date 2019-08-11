using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Liquid",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Description = "The liquid module enables content items to have liquid syntax.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
