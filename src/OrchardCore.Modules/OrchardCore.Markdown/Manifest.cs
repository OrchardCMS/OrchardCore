using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Markdown",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Description = "The markdown module enables content items to have markdown editors.",
    Dependencies = new [] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
