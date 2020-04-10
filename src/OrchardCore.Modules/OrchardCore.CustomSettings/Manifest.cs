using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Custom Settings",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The custom settings modules enables content types to become custom site settings.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Settings"
)]
