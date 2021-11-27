# Getting started with Orchard Core as a NuGet package

In this article, we are going to see how easy it is to create a CMS Web application using the NuGet packages provided by Orchard Core.

## Create an Orchard Core CMS application

In Visual Studio, create a new empty .NET Core web application. Ex: `Cms.Web`. Do not check "Place solution and project in the same directory", because later when you create modules and themes you will want them to live alongside the web application within the solution.

!!! note
    If you want to use the `preview` packages, [configure the OrchardCore Preview url in your Package sources](preview-package-source.md)

To add a reference to the package, right-click on the project and click on `Manage NuGet packages...`, check `Include prerelease` if required. If you added the preview source above, select this from the `Package Source` selection in the top right.  In the `Browse` tab, search for `OrchardCore.Application.Cms.Targets` and `Install` the package.

Open `Program.cs` and modify the file adding this line after the `builder` initialization:

```csharp
builder.Services.AddOrchardCms();
```

After the `app` initialization, replace this line:

```csharp
app.MapGet("/", () => "Hello World!");
```

with this line:

```csharp
app.UseOrchardCore();
```

## Setup your application

Launch your application (Ctrl+F5). The setup page is displayed.

Enter the required information about the site:

- The name of the site. Ex: `Orchard Core`.
- The theme recipe to use. Ex: `Agency`.
- The timezone of the site. Ex: `(+01:00) Europe/Paris`.
- The Sql provider to use. Ex: `SqLite`.
- The name of the admin user. Ex: `admin`.
- The email of the admin. Ex: `foo@bar.com`
- The password and the password confirmation.

Submit the form and your site is generated after a few seconds.

Then, you can access to the admin using the `/admin` url. Enjoy.
