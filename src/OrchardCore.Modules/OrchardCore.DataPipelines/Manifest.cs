using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Data Pipelines",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Design pipelines that read, transform and deliver data.",
    Category = "Data"
)]

[assembly: Feature(
    Id = "OrchardCore.DataPipelines",
    Name = "Data Pipelines",
    Description = "Design pipelines that read data from any data source, transform it, and deliver it as files or content items.",
    Dependencies =
    [
        "OrchardCore.DataSources",
    ],
    Category = "Data"
)]
