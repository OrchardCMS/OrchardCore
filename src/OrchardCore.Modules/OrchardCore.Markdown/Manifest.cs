using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Markdown",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The markdown module enables content items to have markdown editors.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
