using OrchardCore.GitHub;
using OrchardCore.Modules.Manifest;

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
