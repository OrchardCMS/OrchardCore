using OrchardCore.Modules.Manifest;
using OrchardCore.Twitter;

[assembly: Module(
    Name = "Twitter",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "X"
)]

[assembly: Feature(
    Id = TwitterConstants.Features.Twitter,
    Name = "X Integration",
    Category = "X",
    Description = "Provides a TwitterClient and Workflow Activities to integrate with X"
)]

[assembly: Feature(
    Id = TwitterConstants.Features.Signin,
    Name = "Sign in with X",
    Category = "X",
    Description = "Authenticates users with their X Account.",
    Dependencies = [TwitterConstants.Features.Twitter]
)]
