# Getting started with an Orchard Core Theme

In this article, we are going to create a Orchard Core Theme.

We will add it to an existing Orchard Core Cms application [created previously](README).

## Create an Orchard Core Theme

Install the [Code Generation Templates](../../Templates/README), create a folder with the name of your theme (Ex: `MyTheme.OrchardCore`) and execute the command `dotnet new octheme`.

If you want to provide a thumbnail for the theme, add a `Theme.png` in the `wwwwroot` folder.

Then, do not forget to add a reference to the theme in the main Orchard Core Cms Web application.

![image](assets/MyTheme.png)

You can change the properties in the Manifest.cs file:

```csharp
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "MyTheme",
    Author = "My name",
    Website = "https://mywebsite.net",
    Version = "0.0.1",
    Description = "My Orchard Core Theme description."
)]
```

You can now launch the website and the theme will be available in the `Active themes` admin page that allows you to select the default theme.