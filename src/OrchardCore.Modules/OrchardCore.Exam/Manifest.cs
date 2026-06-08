using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Exam",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Exam",
    Name = "Exam",
    Description = "Exam module for question bank management, paper generation, practice and examination.",
    Dependencies = ["OrchardCore.Contents", "OrchardCore.Taxonomies"],
    Category = "Content Management"
)]
