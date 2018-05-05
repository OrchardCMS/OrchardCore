using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Body",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The body module enables content items to have bodies.",
    Dependencies = new [] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
