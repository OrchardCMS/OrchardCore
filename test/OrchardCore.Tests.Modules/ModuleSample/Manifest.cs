using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Module Sample",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Test"
)]

[assembly: Feature(
    Id = "Sample1",
    Name = "Sample 1",
    Description = "Feature with no dependencies.",
    Dependencies = new string[0]
)]

[assembly: Feature(
    Id = "Sample2",
    Name = "Sample 2",
    Description = "Module with dependency one sample 1.",
    Dependencies = new[] { "Sample1" }
)]

[assembly: Feature(
    Id = "Sample3",
    Name = "Sample 3",
    Description = "Module with dependency one sample 2.",
    Dependencies = new[] { "Sample2" }
)]

[assembly: Feature(
    Id = "Sample4",
    Name = "Sample 4",
    Description = "Module with dependency one sample 2.",
    Dependencies = new[] { "Sample2" }
)]
