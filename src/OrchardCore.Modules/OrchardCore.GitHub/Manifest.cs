using OrchardCore.Modules.Manifest;
using OrchardCore.GitHub;

[assembly: Module(
    Name = "GitHub",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "GitHub"
)]

[assembly: Feature(
    Id = GitHubConstants.Features.GitHubAuthentication,
    Name = "GitHub Authentication",
    Category = "GitHub",
    Description = "Authenticates users with their GitHub Account."
)]
