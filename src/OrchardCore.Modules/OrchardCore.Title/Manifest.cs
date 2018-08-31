using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Title",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The title module enables content items to have titles.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
