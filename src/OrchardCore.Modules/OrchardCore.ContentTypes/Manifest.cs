using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Types",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Content Types modules enables the creation and alteration of content types not based on code.",
    Dependencies = new [] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
