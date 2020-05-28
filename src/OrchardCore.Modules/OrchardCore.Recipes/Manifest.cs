using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Recipes",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The Recipes module allows you to execute recipe steps from json files.",
    Dependencies = new[]
    {
        "OrchardCore.Features",
        "OrchardCore.Scripting"
    },
    Category = "Infrastructure",
    IsAlwaysEnabled = true
)]
