using OrchardCore.Modules.Manifest;

[assembly: Module(
    name: "Features",
    author: "The Orchard Team",
    website: "http://orchardproject.net",
    version: "2.0.0"
)]

[assembly: Feature(
    id: "OrchardCore.Features",
    description: "The Features module enables the administrator of the site to manage the installed modules as well as activate and de-activate features.",
    dependencies: "OrchardCore.Resources",
    category: "Infrastructure"
)]

// ... others features
