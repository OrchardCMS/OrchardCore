# Creating a custom admin theme

A custom Admin Theme may be created for Orchard Core.

The default admin theme for Orchard Core is called `TheAdmin`


## What you will build

You will build a custom theme which uses `TheAdmin` as a base theme.


## What you will need

- An existing Orchard Core website that has already been setup.

## Creating an Orchard Core Theme

Create a Orchard Core Theme following the [Create a theme](../../getting-started/theme.md) guide.

## Edit the Manifest.cs

In the root folder of your theme there will be a file called `Manifest.cs`

Edit this file

```csharp
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "MyAdminTheme",
    Author = "My name",
    Website = "https://mywebsite.net",
    Version = "0.0.1",
    Description = "My Orchard Core Admin theme.",
    Tags = new [] { "admin" },
    BaseTheme = "TheAdmin"
)]
```

Add the property `Tags = new [] { "admin" }` and the property `BaseTheme = "TheAdmin"`

The tag allows the theme to be selected as an admin theme.

The `BaseTheme` property means that when the custom admin theme is active Orchard Core Display Management
will search both `TheAdmin` theme and `MyAdminTheme` for template alternates when displaying admin content.

Views in `MyAdminTheme` will override views in `TheAdmin`.

## Enabling your custom admin theme.

From the root of the folder containing both projects, run this command:

`dotnet run --project .\MySite\MySite.csproj`

!!! note
    If you are using the development branch of the templates, run `dotnet restore .\MySite\MySite.csproj --source https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json` before running the application

Your application should now be running and contain the open ports:

```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

Open a browser on <https://localhost:5001>

Enter the Admin section by opening <https://localhost:5001/admin> and logging in.

Using the left menu go to _Design -> Themes_, search for your theme, __MyAdminTheme__, and select __Make Current__.

Now your admin theme is enabled.

From here you can create templates, or use `placement.json` to alter shapes rendered in the admin.

## Summary

You just learned how to create a custom admin theme.
