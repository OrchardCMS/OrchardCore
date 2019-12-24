using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Features",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Features",
    Name = "Features",
    Description = "The Features module enables the administrator of the site to manage the installed modules as well as activate and de-activate features.",
    Dependencies = new [] { "OrchardCore.Resources" },
    Category = "Infrastructure",
    IsAlwaysEnabled = true
)]
