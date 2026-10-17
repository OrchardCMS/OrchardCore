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
    Id = "OrchardCore.DataPipelines.Core",
    Name = "Data Pipelines Core",
    Description = "Provides the data sources, file formats, formula functions and data transforms that data pipelines and other features, such as reports, share.",
    EnabledByDependencyOnly = true,
    Category = "Data"
)]

[assembly: Feature(
    Id = "OrchardCore.DataPipelines",
    Name = "Data Pipelines",
    Description = "Design pipelines that read data from any data source, transform it, and deliver it to the media library, download links, email, web APIs and content items. The steps of the media library, email, content items and workflows are available when those features are enabled.",
    Dependencies =
    [
        "OrchardCore.DataPipelines.Core",
        "OrchardCore.Users",
    ],
    Category = "Data"
)]
