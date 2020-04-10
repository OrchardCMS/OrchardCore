using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Taxonomies",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The taxonomies module provides a way to categorize content items.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
