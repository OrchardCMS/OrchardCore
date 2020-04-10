using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Types",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "Content Types modules enables the creation and alteration of content types not based on code.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
