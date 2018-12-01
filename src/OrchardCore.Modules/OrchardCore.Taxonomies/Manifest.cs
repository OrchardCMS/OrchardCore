using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Taxonomies",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The taxonomies modules provides a way to categorize content items.",
    Dependencies = new [] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
