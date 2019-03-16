using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Themes",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Description = "Themes.",
    Dependencies = new [] { "OrchardCore.Admin" },
    Category = "Theming"
)]
