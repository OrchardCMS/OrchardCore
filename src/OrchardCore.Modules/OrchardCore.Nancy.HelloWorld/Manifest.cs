using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Nancy HelloWorld",
    Dependencies = new string[]
    {
        "OrchardCore.Nancy"
    }
)]
