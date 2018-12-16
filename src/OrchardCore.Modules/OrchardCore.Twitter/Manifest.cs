using OrchardCore.Modules.Manifest;
using OrchardCore.Twitter;

[assembly: Module(
    Name = "Twitter",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Category = "Twitter"
)]

[assembly: Feature(
    Id = TwitterConstants.Features.TwitterSignin,
    Name = "Sign in with Twitter",
    Category = "Twitter",
    Description = "Authenticates users with their Twitter Account."
)]
