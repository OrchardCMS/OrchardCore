using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "GraphQL",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Apis using the GraphQL Specification.",
    Dependencies = ["OrchardCore.Navigation"],
    Category = "Api"
)]
