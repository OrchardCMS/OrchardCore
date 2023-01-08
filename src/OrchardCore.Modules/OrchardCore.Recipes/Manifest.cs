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
        "OrchardCore.Recipes.Core",
        "OrchardCore.Scripting",
    },
    Category = "Infrastructure",
    IsAlwaysEnabled = true
)]

[assembly: Feature(
    Id = "OrchardCore.Recipes.Core",
    Name = "Recipes",
    Description = "Provides recipe services.",
    Category = "Infrastructure",
    EnabledByDependencyOnly = true
)]
