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
    Dependencies =
    [
        "OrchardCore.Recipes.Core",
        "OrchardCore.Scripting",
    ],
    Category = "Infrastructure",
    IsAlwaysEnabled = true
)]

[assembly: Feature(
    Id = "OrchardCore.Recipes.Default",
    Name = "Default Tenant Recipes",
    Description = "Provides the recipes that are only available to the Default tenant, like the SaaS setup recipe.",
    Dependencies =
    [
        "OrchardCore.Recipes",
    ],
    Category = "Infrastructure",
    DefaultTenantOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Recipes.Core",
    Name = "Recipes Core Services",
    Description = "Provides recipe core services.",
    Category = "Infrastructure",
    EnabledByDependencyOnly = true
)]
