using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Media",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Media",
    Name = "Media",
    Description = "The media module adds media management support.",
    Dependencies = new []
    {
        "OrchardCore.ContentTypes"
    },
    Category = "Content Management"
)]
