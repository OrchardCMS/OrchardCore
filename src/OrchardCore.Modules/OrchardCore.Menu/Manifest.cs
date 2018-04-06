using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Menu",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The Menu module provides menu management features.",
    Dependencies = new []
    {
        "OrchardCore.Contents",
        "OrchardCore.Title",
        "OrchardCore.Alias"
    },
    Category = "Navigation"
)]
