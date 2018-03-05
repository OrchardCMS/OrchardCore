using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Preview",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The content Preview module enables live content edition and content preview.",
    Dependencies = new [] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
