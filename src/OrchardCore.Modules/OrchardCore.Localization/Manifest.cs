using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Localization",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Provides support for UI localization.",
    Category = "Internationalization",
    Dependencies = new[] { "OrchardCore.Settings" }
)]
