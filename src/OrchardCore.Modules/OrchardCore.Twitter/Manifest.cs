using OrchardCore.Modules.Manifest;
using OrchardCore.Twitter;

[assembly: Module(
    Name = "Twitter",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Category = "Twitter"
)]

[assembly: Feature(
    Id = TwitterConstants.Features.Twitter,
    Name = "Twitter Integration",
    Category = "Twitter",
    Description = "Provides a TwitterClient and Workflow Activities to integrate with twitter"
)]

[assembly: Feature(
    Id = TwitterConstants.Features.Signin,
    Name = "Sign in with Twitter",
    Category = "Twitter",
    Description = "Authenticates users with their Twitter Account.",
    Dependencies = new[] { "OrchardCore.Twitter" }
)]
