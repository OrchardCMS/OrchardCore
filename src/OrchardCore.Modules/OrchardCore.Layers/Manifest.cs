using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Layers",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2"
)]

[assembly: Feature(
    Id = "OrchardCore.Layers",
    Name = "Layers",
    Description = "Enables users to render Widgets across pages of the site based on conditions.",
    Dependencies = new[]
    {
        "OrchardCore.Widgets",
        "OrchardCore.Scripting"
    },
    Category = "Content"
)]
