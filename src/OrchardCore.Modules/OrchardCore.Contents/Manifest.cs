using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Contents",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
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
    Id = "OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget",
    Name = "Export Content To Deployment Target",
    Description = "Adds an export to deployment target action to the content items list.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
        "OrchardCore.Deployment",
        "OrchardCore.Recipes.Core",
    },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Contents.Deployment.AddToDeploymentPlan",
    Name = "Add Content To Deployment Plan",
    Description = "Adds an add to deployment plan action to the content items list.",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.Deployment" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Contents.Deployment.Download",
    Name = "View Or Download Content As JSON",
    Description = "View or download content as JSON from the content items list.",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.Deployment" },
    Category = "Content Management"
)]
