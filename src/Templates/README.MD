# Orchard Core Templates

Orchard Core Templates uses `dotnet new` templates configurations for creating new websites, themes and modules from command shell. Our configurations have been implemented by using examples that you can find there :  
https://github.com/dotnet/dotnet-template-samples

## Create a new website

### From Command Shell (automated way)

#### Initialize module template:

```CMD
dotnet new -i "OrchardCore.Templates.Cms.Web"  
```

#### Module commands:

```CMD
dotnet new ocweb  

dotnet new ocweb -AddNLog true  
```

### From Visual Studio (manual way)

Fire up Visual Studio, create a new solution file (.sln) by creating a new ASP.NET Core Web Application : 

![image](/src/docs/assets/images/orchard-screencast-1.gif)

Now that we created a new Web Application we need to add proper dependencies so that this new Web Application be targetted as an Orchard Core application.

[See adding Orchard Core Nuget Feed](#adding-orchard-core-nuget-feed)

![image](/src/docs/assets/images/orchard-screencast-2.gif)

Finally we will need to register Orchard CMS service in our Startup.cs file like this : 

```C#
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MyNewWebsite
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOrchardCore();
        }
    }
}
```

## Create a new module

### From Command Shell (automated way)

#### Initialize module template:

```CMD
dotnet new -i "OrchardCore.Templates.Module"  
```

#### Module commands:

```CMD
dotnet new ocmodule -n "ModuleName.OrchardCore"

dotnet new ocmodule -n "ModuleName.OrchardCore" -PartName "TestPart"

dotnet new ocmodule -n "ModuleName.OrchardCore" -PartName "TestPart" -AddPart true
```

### From Visual Studio (manual way)

Fire up Visual Studio, open Orchard Core solution file (.sln), select OrchardCore.Modules folder, right click and select "add --> new project" and create a new .NET Standard Class Library:

![image](/src/docs/assets/images/38450533-6c0fbc98-39ed-11e8-91a5-d26a1105b91a.png)

For marking this new Class Library as an Orchard Module we will now need to reference OrchardCore.Module.Targets Nuget package.

[See adding Orchard Core Nuget Feed](#adding-orchard-core-nuget-feed)

Each of these "*.Targets" Nuget packages are used to mark a Class Library as a specific Orchard Core functionality. OrchardCore.Module.Targets is the one we are interested in for now. We will mark our new Class Library as a module by adding OrchardCore.Module.Targets as a dependency. For doing so you will need to right click on MyModule.OrchardCore project and select "Manage Nuget Packages" option. To find the packages in Nuget Package Manager you will need to check "include prerelease" and make sure you have Orchard Core feed that we added earlier selected. Once you have found it click on the Install button on the right panel next to Version : Latest prerelease x.x.x.x

![image](/src/docs/assets/images/38450558-f4b83098-39ed-11e8-93c7-0fd9e5112dff.png)

Once done your new module will look like this : 

![image](/src/docs/assets/images/38450628-31c8e2b0-39ef-11e8-9de7-c15f0c6544c5.png)

For Orchard Core to identify this module it will now require a Manifest.cs file. Here is an example of that file:

```C#
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "TemplateModule.OrchardCore",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "0.0.1",
    Description = "Template module."
)]

```

Please refer to Orchard Core documentation about Manifest.cs files for further details.  
TODO : Orchard Core Manifest documentation. Previous documentation as a reference link below.  
http://docs.orchardproject.net/en/latest/Documentation/Manifest-files/

For this module to start we now will need to add a startup.cs file to our new module. See this file as an example:  
https://github.com/OrchardCMS/OrchardCore/blob/dotnetNewTemplates/src/Templates/OrchardCore.Templates.Module/Startup.cs

Last step is to add our new module to the OrchardCore.Cms.Web project as a reference for including it as part as our website modules. After that, you should be all set for starting building your custom module. You can refer to our template module for examples of what's basically needed normally. https://github.com/OrchardCMS/OrchardCore/blob/dotnetNewTemplates/src/Templates/OrchardCore.Templates.Module/

## Create a new theme

### From Command Shell (automated way)

#### Initialize theme template:

```CMD
dotnet new -i "OrchardCore.Templates.Theme"  
```

#### Theme commands:

`dotnet new octheme -n "ThemeName.OrchardCore"`

### From Visual Studio (manual way)

Should be the same procedure as with modules but instead we need to reference OrchardCore.Theme.Targets and Manifest.cs file slightly differ : 

```C#
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "TemplateTheme.OrchardCore",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "0.0.1",
    Description = "The TemplateTheme."
)]
```

## All Commands

**Initialize:**

```CMD
dotnet new -i "OrchardCore.Templates.Cms.Web"  

dotnet new -i "OrchardCore.Templates.Module"   

dotnet new -i "OrchardCore.Templates.Theme"  
```

Alternatively from an empty folder not containing any nuget.config file you can use `dotnet new --nuget-source` available with .NET Core 2.1 only :  

```CMD
dotnet new -i "OrchardCore.Templates.Cms.Web" --nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json  

dotnet new -i "OrchardCore.Templates.Module" --nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json  

dotnet new -i "OrchardCore.Templates.Theme" --nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json  
```

**After initializing :**

```CMD
dotnet new ocweb

dotnet new ocmodule -n "Module.OrchardCore"

dotnet new octheme -n "Theme.OrchardCore"

dotnet new ocmodule -n "Module.OrchardCore" -PartName "TestPart"

dotnet new ocweb -AddNLog true

dotnet new ocmodule -n "Module.OrchardCore" -PartName "TestPart" -AddPart true
```


**Reinitialize templates :**

`dotnet new --debug:reinit`

## Adding Orchard Core Nuget Feed

![image](/src/docs/assets/images/38450242-886933f6-39e8-11e8-896c-1f807e5530a0.png)

#### MyGet:

Preview: https://www.myget.org/gallery/orchardcore-preview

Feed: https://www.myget.org/F/orchardcore-preview/api/v3/index.json



To be able to use these feeds in Visual Studio your will need to add them to the Nuget package source settings that can be found by going to Visual Studio Tools menu under Nuget Package Manager --> Package Manager Settings.

![image](/src/docs/assets/images/38450422-63670f1c-39eb-11e8-9c14-0743f0a4da42.png)