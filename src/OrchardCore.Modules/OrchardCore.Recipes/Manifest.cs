using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Recipes",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Provides Orchard Recipes.",
    Dependencies = new []
    {
        "OrchardCore.Features",
        "OrchardCore.Scripting"
    },
    Category = "Infrastructure"
)]
