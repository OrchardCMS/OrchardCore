using OrchardCore.Modules.Manifest;
using OrchardCore.Facebook;

[assembly: Module(
    Name = "Facebook",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Facebook"
)]

[assembly: Feature(
    Id = FacebookConstants.Features.Core,
    Name = "Facebook",
    Category = "Facebook",
    Description = "Registers the core components used by the Facebook features."
)]

[assembly: Feature(
    Id = FacebookConstants.Features.Login,
    Name = "Facebook Login",
    Category = "Facebook",
    Description = "Authenticates users from facebook.",
    Dependencies = new[] { FacebookConstants.Features.Core }
)]

[assembly: Feature(
    Id = FacebookConstants.Features.Widgets,
    Name = "Facebook Social Plugins Widgets",
    Category = "Facebook",
    Description = "Integrates Facebook Social Plugins as predefined widgets",
    Dependencies = new[] { FacebookConstants.Features.Core, "OrchardCore.Widgets" }
)]
