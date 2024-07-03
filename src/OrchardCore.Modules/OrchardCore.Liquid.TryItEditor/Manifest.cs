using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Liquid TryIt Editor",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Liquid TryIt editor for OrchardCore",
    Dependencies = [ "OrchardCore.Liquid" ],
    Category = "Content Management"
)]
