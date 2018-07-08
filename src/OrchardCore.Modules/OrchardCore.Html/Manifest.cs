using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Html",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The Html module enables content items to have Html bodies.",
    Dependencies = new [] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
