using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Sitemaps",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Provides dynamic sitemap generation services",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.Contents" }
)]