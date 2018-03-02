using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Contents",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The contents module enables the edition and rendering of content items.",
    Dependencies = new []
    {
        "OrchardCore.Settings",
        "OrchardCore.Liquid"
    },
    Category = "Content Management"
)]
