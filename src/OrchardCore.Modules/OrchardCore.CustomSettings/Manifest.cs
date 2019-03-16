using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Custom Settings",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Description = "The custom settings modules enables content types to become custom site settings.",
    Dependencies = new [] { "OrchardCore.Contents" },
    Category = "Settings"
)]
