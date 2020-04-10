using OrchardCore.Modules.Manifest;
using OrchardCore.GitHub;

[assembly: Module(
    Name = "GitHub",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Category = "GitHub"
)]
[assembly: Feature(
    Id = GitHubConstants.Features.GitHubAuthentication,
    Name = "GitHub Authentication",
    Category = "GitHub",
    Description = "Authenticates users with their GitHub Account."
)]
