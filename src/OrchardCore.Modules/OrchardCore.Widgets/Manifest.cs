using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Widgets",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Widgets",
    Description = "Provides a part allowing content items to render Widgets in theme zones.",
    Dependencies = new [] { "OrchardCore.ContentTypes" },
    Category = "Content"
)]
