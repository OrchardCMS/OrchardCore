using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Localization",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "1.0.0",
    Description = "The Content Localization module provides a part that allows you to easily localize content",
    Dependencies = new[] { "OrchardCore.ContentTypes", "OrchardCore.Localization" }, 
    Category = "Internationalization"
)]
