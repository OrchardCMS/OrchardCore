using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Html",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The Html module enables content items to have Html bodies.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
