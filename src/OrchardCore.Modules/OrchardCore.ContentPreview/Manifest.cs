using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Preview",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The content Preview module enables live content edition and content preview.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
