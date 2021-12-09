using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Html",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The Html module enables content items to have Html bodies.",
    Dependencies = new[] { "OrchardCore.ContentTypes", "OrchardCore.Shortcodes" },
    Category = "Content Management"
)]
