using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Localization",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "Provides support for UI localization.",
    Category = "Internationalization",
    Dependencies = new[] { "OrchardCore.Settings" }
)]
