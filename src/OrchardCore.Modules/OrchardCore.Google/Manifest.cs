using OrchardCore.Modules.Manifest;
using OrchardCore.Google;

[assembly: Module(
    Name = "Google",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Category = "Google"
)]

[assembly: Feature(
    Id = GoogleConstants.Features.GoogleAuthentication,
    Name = "Google Authentication",
    Category = "Google",
    Description = "Authenticates users with their Google Account."
)]
