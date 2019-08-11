using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Widgets",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Widgets",
    Name = "Widgets",
    Description = "Provides a part allowing content items to render Widgets in theme zones.",
    Dependencies = new [] { "OrchardCore.ContentTypes" },
    Category = "Content"
)]
