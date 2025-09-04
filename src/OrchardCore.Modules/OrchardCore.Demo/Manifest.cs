using OrchardCore.Modules.Manifest;
using OrchardCore.Users;

[assembly: Module(
    Name = "Orchard Demo",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Name = "Orchard Demo",
    Id = "OrchardCore.Demo",
    Description = "Test",
    Category = "Samples",
    Dependencies =
    [
        UserConstants.Features.Users,
        "OrchardCore.Contents",
    ]
)]

[assembly: Feature(
    Id = "OrchardCore.Demo.Foo",
    Name = "Orchard Foo Demo",
    Description = "Foo feature sample.",
    Category = "Samples"
)]
