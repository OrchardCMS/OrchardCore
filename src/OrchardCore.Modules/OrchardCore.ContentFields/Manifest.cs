using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Fields",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Content Fields module adds common content fields to be used with your custom types.",
    Dependencies = new [] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
