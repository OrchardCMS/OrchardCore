using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Widgets",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2"
)]
[assembly: Feature(
    Id = "OrchardCore.Widgets",
    Name = "Widgets",
    Description = "Provides a part allowing content items to render Widgets in theme zones.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content"
)]
