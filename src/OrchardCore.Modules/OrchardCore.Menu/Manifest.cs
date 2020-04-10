using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Menu",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The Menu module provides menu management features.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
        "OrchardCore.Title",
        "OrchardCore.Alias"
    },
    Category = "Navigation"
)]
