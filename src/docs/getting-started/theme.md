# Getting started with an Orchard Core Theme

In this article, we are going to create an Orchard Core Theme by adding it to an existing Orchard Core CMS application [created previously](README).

## Create an Orchard Core Theme

- Install the [Code Generation Templates](../../Templates/README) 
- Create a folder with the name of your theme (Ex: `MyTheme.OrchardCore`) and open it
- Execute the command `dotnet new octheme`
- Add a reference to the theme from the main Orchard Core CMS Web application

A thumbnail can also be created by adding a `Theme.png` in the `wwwwroot` folder.

![image](assets/MyTheme.png)

The properties of the theme can be changed in the __Manifest.cs__ file:

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

The theme should be available in the `Active themes` admin page, and can be set as the default theme.
