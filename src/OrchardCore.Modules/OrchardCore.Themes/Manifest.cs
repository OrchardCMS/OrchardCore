using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Themes",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Themes.",
    Dependencies = new [] { "OrchardCore.Admin" },
    Category = "Theming"
)]
