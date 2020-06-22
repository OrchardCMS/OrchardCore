using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Markdown",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The markdown module enables content items to have markdown editors.",
    Dependencies = new[] { "OrchardCore.ContentTypes", "OrchardCore.ShortCodes" },
    Category = "Content Management"
)]
