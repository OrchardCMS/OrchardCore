using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Title",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The title module enables content items to have titles.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
