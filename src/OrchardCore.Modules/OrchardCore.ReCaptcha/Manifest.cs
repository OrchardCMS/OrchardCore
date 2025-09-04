using OrchardCore.Modules.Manifest;
using OrchardCore.Users;

[assembly: Module(
    Name = "ReCaptcha",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion)]

[assembly: Feature(
    Id = "OrchardCore.ReCaptcha",
    Name = "ReCaptcha",
    Category = "Security",
    Description = "Provides core ReCaptcha functionality.")]

[assembly: Feature(
    Id = "OrchardCore.ReCaptcha.Users",
    Name = "ReCaptcha Users",
    Description = "Provides ReCaptcha functionality to harness login, register, forgot password and forms against robots.",
    Category = "Security",
    Dependencies =
    [
        "OrchardCore.ReCaptcha",
        UserConstants.Features.Users,
    ])]
