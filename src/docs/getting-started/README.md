# Getting started with Orchard Core as a NuGet package

In this article, we are going to see how easy it is to create a CMS Web application using the NuGet packages provided by Orchard Core.

You can find the original blog post written by Chris Payne here:  
<http://ideliverable.com/blog/getting-started-with-orchard-core-as-a-nuget-package>

## Create an Orchard Core CMS application

In Visual Studio, create a new empty .NET Core web application. Ex: `Cms.Web`.

If you want to use the `dev` packages, add this OrchardCore-preview MyGet url to your NuGet sources:  
<https://www.myget.org/F/orchardcore-preview/api/v3/index.json>

Right-click on the project and click on `Manage NuGet packages...`.
In the `Browse` tab, search for `OrchardCore.Application.Cms.Targets` and `Install` the package.

Open `Startup.cs` and modify the `ConfigureServices` method by adding this line:

```csharp
services.AddOrchardCms();
```

In the `Configure` method, replace this block:

```csharp
app.Run(async (context) =>
{
    await context.Response.WriteAsync("Hello World!");
});
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

## Custom operations in the database

Sometimes you want to do custom things in the database like creating your own tables and performing CRUD operations on them. You can acccess the underling IDbConnection by injecting the IDbConnectionAccessor interface.  Just use your favorite ORM and your are good to go!

```
using OrchardCore.Data.Abstractions;

namespace SomeNamespace
{
    public class SomeController : Controller
    {
        public SomeController(IDbConnectionAccessor accessor)
        {
            _accessor = accessor;
        }
        
        public async Task<ActionResult> Index()
        {
            using (var connection = await _accessor.GetConnectionAsync())
            {
                //Do your database read/write operations here.
            }
        }
    
    }
}
```

