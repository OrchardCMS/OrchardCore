using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Localization",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Description = "Provides support for UI localization.",
    Category = "Internationalization",
    Dependencies = new[] { "OrchardCore.Settings" }
)]
