using OrchardCore.Modules.Manifest;
using OrchardCore.GitHub;

[assembly: Module(
    Name = "Github",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Category = "Github"
)]

[assembly: Feature(
    Id = GitHubConstants.Features.GithubAuthentication,
    Name = "Github Authentication",
    Category = "Github",
    Description = "Authenticates users with their Github Account."
)]
