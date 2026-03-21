using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OpenApi",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.OpenApi",
    Name = "OpenApi",
    Description = "Microsoft.AspnetCore.OpenApi module for Orchard Core.",
    Category = "Api"
)]

[assembly: Feature(
    Id = "OrchardCore.OpenApi.SwaggerUI",
    Name = "OpenApi Swagger UI",
    Description = "Enables the Swagger UI interactive API explorer at ~/swagger.",
    Dependencies =
    [
        "OrchardCore.OpenApi"
    ],
    Category = "Api"
)]

[assembly: Feature(
    Id = "OrchardCore.OpenApi.ReDocUI",
    Name = "OpenApi ReDoc UI",
    Description = "Enables the ReDoc read-only API documentation at ~/redoc.",
    Dependencies =
    [
        "OrchardCore.OpenApi"
    ],
    Category = "Api"
)]

[assembly: Feature(
    Id = "OrchardCore.OpenApi.ScalarUI",
    Name = "OpenApi Scalar UI",
    Description = "Enables the Scalar modern API reference at ~/scalar/v1.",
    Dependencies =
    [
        "OrchardCore.OpenApi"
    ],
    Category = "Api"
)]
