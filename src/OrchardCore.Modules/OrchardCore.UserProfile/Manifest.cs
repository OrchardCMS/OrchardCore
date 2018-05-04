using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Users Profile",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Users profile (settings)",
    Dependencies = new[] { "OrchardCore.Admin" },
    Category = "Settings"
)]
