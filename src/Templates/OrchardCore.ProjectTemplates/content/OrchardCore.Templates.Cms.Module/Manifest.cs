using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrchardCore.Templates.Cms.Module",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = "0.0.1",
    Description = "OrchardCore.Templates.Cms.Module",
#if (AddPart)
    Dependencies = new[] { "OrchardCore.Contents" },
#endif
    Category = "Content Management"
)]
