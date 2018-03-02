using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Layers",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Layers",
    Description = "Enables users to render Widgets across pages of the site based on conditions.",
    Dependencies = new []
    {
        "OrchardCore.Widgets",
        "OrchardCore.Scripting"
    },
    Category = "Content"
)]
