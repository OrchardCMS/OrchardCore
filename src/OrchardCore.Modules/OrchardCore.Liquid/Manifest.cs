using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Liquid",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The liquid module enables content items to have liquid syntax.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
