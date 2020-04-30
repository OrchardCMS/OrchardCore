using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Contents",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Contents",
    Name = "Contents",
    Description = "The contents module enables the edition and rendering of content items.",
    Dependencies = new[]
    {
        "OrchardCore.Settings",
        "OrchardCore.Liquid"
    },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Contents.FileContentDefinition",
    Name = "File Content Definition",
    Description = "Stores Content Definition in a local file.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Contents.ExportContentToDeploymentTarget",
    Name = "Export Content To Deployment Target",
    Description = "Adds a export to deployment target action to the content item list.",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.Deployment" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Contents.AddToDeploymentPlan",
    Name = "Add Content To Deployment Plan",
    Description = "Adds a add to deployment plan action for the content item list.",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.Deployment" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Contents.ViewAsJson",
    Name = "View Or Export Content As JSON",
    Description = "View or export content as JSON from the content item list.",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.Deployment" },
    Category = "Content Management"
)]
