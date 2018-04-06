using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Templates",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The Templates module provides a way to write custom shape templates from the admin.",
    Dependencies = new [] { "OrchardCore.Liquid" },
    Category = "Development"
)]
