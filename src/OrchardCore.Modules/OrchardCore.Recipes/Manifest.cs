using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Recipes",
    Name = "Recipes",
    Description = "The Recipes module allows you to execute recipe steps from json files.",
    Dependencies = new[]
    {
        "OrchardCore.Scripting",
        "OrchardCore.Recipes.Core"
    },
    Category = "Infrastructure",
    IsAlwaysEnabled = true
)]

[assembly: Feature(
    Id = "OrchardCore.Recipes.Core",
    Name = "Recipes",
    Description = "Provides recipe services.",
    Dependencies = new[]
    {
        "OrchardCore.Recipes.Core"
    },
    Category = "Infrastructure",
    EnabledByDependencyOnly = true
)]
