using OrchardCore.Modules.Manifest;
using OrchardCore.Facebook;

[assembly: Module(
    Name = "Facebook",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = FacebookConstants.Features.Login,
    Name = "Facebook Login",
    Category = "Facebook",
    Description = "Auhenticates users from facebook.",
    Dependencies = new[] { FacebookConstants.Features.Core }
)]

[assembly: Feature(
    Id = FacebookConstants.Features.Core,
    Name = "Facebook Core Components",
    Category = "Facebook",
    Description = "Registers the core components used by the Facebook module."
)]