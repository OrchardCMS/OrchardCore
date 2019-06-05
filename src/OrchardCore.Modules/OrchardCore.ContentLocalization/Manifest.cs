using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Localization",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "1.0.0",
    Description = "Provides a part that allows to localize content items.",
    Dependencies = new[] { "OrchardCore.ContentTypes", "OrchardCore.Localization" },
    Category = "Internationalization"
)]
