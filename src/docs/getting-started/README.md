# Getting started with Orchard Core as a NuGet package

In this article, we are going to see how easy it is to create a CMS Web application using the NuGet packages provided by Orchard Core.

## Create an Orchard Core CMS application

In Visual Studio, create a new empty .NET Core web application. Ex: `Cms.Web`. Do not check "Place solution and project in the same directory", because later when you create modules and themes you will want them to live alongside the web application within the solution.

!!! note
    If you want to use the `preview` packages, [configure the OrchardCore Preview url in your Package sources](preview-package-source.md)

To add a reference to the package, right-click on the project and click on `Manage NuGet packages...`, check `Include prerelease` if required. If you added the preview source above, select this from the `Package Source` selection in the top right.  In the `Browse` tab, search for `OrchardCore.Application.Cms.Targets` and `Install` the package.

### Getting Started with `Program.cs` Only Using .NET 6 Framework?
!!! tip
    When starting a new project using `.NET 6` framework, you'll notice that the created project does not have a `Startup` class as it did in previous versions of the .NET framework.

Open `Program.cs` file. Remove the following line "if exists"

```csharp
builder.Services.AddRazorPages();
```

Add the following line 

```csharp
builder.Services.AddOrchardCms();
```

Additionally, remove the following lines

```csharp
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
```
Lastly, add the following line to the request pipeline

```csharp
app.UseOrchardCore();
```

When you are done, the `Program.cs` file will something like this

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrchardCms();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseOrchardCore();

app.Run();
```

### Getting Started Using `Program.cs` file?

Open `Program.cs` file, then add the OrchardCore CMS services by adding this line:

```csharp
builder.Services.AddOrchardCms();
```

After building the `WebApplication`, replace this line:

```csharp
app.MapGet("/", () => "Hello World!");
```

with this line:

```csharp
app.UseOrchardCore();
```

Finally, remove the default `Pages` and/or `Views` folder to allow OrchardCore to render the views from the active theme.

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
