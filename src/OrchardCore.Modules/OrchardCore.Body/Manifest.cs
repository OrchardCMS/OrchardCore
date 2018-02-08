using OrchardCore.Modules.Manifest;

[assembly: Module(
    name: "Body",
    author: "The Orchard Team",
    website: "http://orchardproject.net",
    version: "2.0.0",
    description: "The body module enables content items to have bodies.",
    dependencies: "OrchardCore.ContentTypes, OrchardCore.Liquid",
    category: "Content Management"
)]
