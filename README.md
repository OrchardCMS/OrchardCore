# Orchard 2 [![BSD-3-Clause License](https://img.shields.io/badge/license-BSD--3--Clause-blue.svg)](LICENSE.txt)

Orchard 2 is the implementation of [Orchard CMS](https://github.com/OrchardCMS/Orchard) in [ASP.NET Core](http://www.asp.net/vnext) (also known as DNX). You can check out the [Orchard 2 presentation from the last Orchard Harvest](https://www.youtube.com/watch?v=TK6a_HfD0O8) to get an introductory overview of its features and goals.

[![Join the chat at https://gitter.im/OrchardCMS/Orchard2](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OrchardCMS/Orchard2?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Build Statuses

| Build server| Platform       | Status                                                                                                                                                                  |
|-------------|----------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| AppVeyor    | Windows        | [![AppVeyor](https://ci.appveyor.com/api/projects/status/ccmxpn9l3q377jhg/branch/master?svg=true)](https://ci.appveyor.com/project/alexbocharov/orchard2/branch/master) |
| Travis      | Linux / OS X   | [![Travis](https://travis-ci.org/alexbocharov/Orchard2.svg?branch=master)](https://travis-ci.org/alexbocharov/Orchard2)                                                 |
| MyGet       | Windows        | [![brochard MyGet Build Status](https://www.myget.org/BuildSource/Badge/brochard?identifier=098718e3-f53d-4bcd-b29e-cb9da86823c0)](https://www.myget.org/)              |

## Orchard CMS

Orchard is a free, [open source](https://github.com/OrchardCMS/Orchard), community-focused Content Management System built on the ASP.NET MVC platform.

## Getting Started

- Clone the repository using the command `git clone https://github.com/OrchardCMS/Orchard2.git` and checkout the `master` branch. 
- Delete `%LocalAppData%\Microsoft\dotnet` – The shared runtime dotnet installer doesn’t account for the old CLI structure.
- Delete `.build` (or run git clean -xdf) to get the latest KoreBuild
- Delete `C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\dotnet-test-xunit`
- Run the `build.cmd` file included in the repository to dotnet CLI and build the solution.
- Next navigate to `D:\Orchard2\src\Orchard.Web` or where ever your retrospective folder is on the command line in Administrator mode.

### Using Kestrel

- Call `dotnet run`.
- Then open the `http://localhost:5001` URL in your browser.

### Using Console

- Call `dotnet run`.
- From here you can now execute commands in a similar fashion as before.

### Creating a host

When running Orchard 2, you need a client. The default implementation is to have a client talk to a host.

The client can be any project that creates the host.

To create the host in a web project you would do:

```c#
public class Startup {
    public IServiceProvider ConfigureServices(IServiceCollection services) {
        return services
            // AddHostSample is where the magic is done. This extension method lives in the Host (Orchard.Hosting.Web)
            .AddHostSample()
            .BuildServiceProvider();
    }
}
```

The host has a small wrapper:

```c#
public static IServiceCollection AddHostSample(this IServiceCollection services) {
    // This will setup all your core services for a host
    return services.AddHost(internalServices => {
        // The core of the host
        internalServices.AddHostCore();
        //... All extra things you want registered so that you don't have to touch the core host.
    });
```

### Additional module locations

Additional locations for module discovery can be added in your client setup:

```c#
public class Startup {
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddWebHost();

        // Add folders the easy way
        services.AddModuleFolder("Core/Orchard.Core");
        services.AddModuleFolder("Modules");
        services.AddThemeFolder("Themes");

        // Add folders the more configurable way
        services.Configure<ExtensionHarvestingOptions>(options => {
            var expander = new ModuleLocationExpander(
                DefaultExtensionTypes.Module,
                new[] { "Core/Orchard.Core", "Modules" },
                "Module.txt"
                );

            options.ModuleLocationExpanders.Add(expander);
        });
    });
}
```

### Tenant Configuration

All tenant configuration lives in `src\Orchard.Web\App_Data\Sites\Default` within settings files, e.g. `Settings.txt`:

```yaml
State: Running
Name: Default
RequestUrlHost: localhost:5001
RequestUrlPrefix:
```

However, you can override these values within a .json or .xml file. The order of precendence is:
Settings.txt -> Settings.xml -> Settings.json

You can also override the 'Sites' folder in your client setup

```c#
public class Startup {
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddWebHost();

        // Change the folder name here
        services.ConfigureShell("Sites");
    });
}
```

### Orchard file System

Orchard now has a build in file system that is scoped to the running site. To use this, you just need to inject in IOrchardFileSystem.

You use non virtual paths for access, so for example, lets say you have this folder.

> D:\Orchard2\src\Orchard.Web\Modules\Orchard.Lists\Module.txt

and in you code you want to read that file,

```c#
public void GetMeThatFile()
{
  var fileText = _fileSystem.ReadFile("Modules\Orchard.Lists\Module.txt");
  // The physical path will be D:\Orchard2\src\Orchard.Web\Modules\Orchard.Lists\Module.txt
}
```

The file system is scoped to the Orchard.Web folder by default. If however you want another filesystem, you can create a new one elsewhere.

```c#
public void CreateMeAFileSystem()
{
  var root = "C:\MyFileSystemRootPath";
  var fileSystem = new OrchardFileSystem(
    root,
    new PhysicalFileProvider(root),
    _logger);

  // now if I get my module file..
  var fileText = fileSystem.ReadFile("Modules\Orchard.Lists\Module.txt");
  // The physical path will be C:\MyFileSystemRootPath\Modules\Orchard.Lists\Module.txt
}
```

If you would like to deal with files within a particular Extension Folder you can do this:

```c#
public void GetMePlacement()
{
  // First get the extension.
  ExtensionDescriptor extensionDescriptor = _extensionManager.GetExtension("Orchard.Lists");

  // Second use the extension to get the placement info file
  IFileInfo placementInfoFile = _fileSystem
    .GetExtensionFileProvider(extensionDescriptor, _logger)
    .GetFileInfo("Placement.info");
}
```

###Testing

We currently use XUnit to do unit testing.

###Running under linux

Orchard 2 can be run using dotnet CLI under Ubuntu 14.04 LTS for now. Here are the steps to get it running : 

1. Install .NET Core
    * Add the new apt-get feed

        In order to install .NET Core on Ubuntu, we need to first set up the apt-get feed that hosts the package we need.

        Note: as of now, the below instructions work on Ubuntu 14.04 and derivatives. New versions are coming up soon! Also, please be aware that this feed is our development feed. As we stabilize we will change feeds where deb packages are stored.

        ```
        sudo sh -c 'echo "deb [arch=amd64] http://apt-mo.trafficmanager.net/repos/dotnet/ trusty main" > /etc/apt/sources.list.d/dotnetdev.list'
        sudo apt-key adv --keyserver apt-mo.trafficmanager.net --recv-keys 417A0893
        sudo apt-get update
        ```

    * Install .NET Core (dotnet CLI)

        Installing .NET Core is a simple thing on Ubuntu. The below will install the package and all of its dependencies.

        ```
        sudo apt-get install dotnet-dev-1.0.0-rc2-002543  //default version can change over time
        ```
        
        * Other usefull commands
        
            This will get you a list of dotnet version that you can install
        
            ```
            sudo apt-cache search dotnet
            ```
            
            This will remove any installed version of the dotnet-dev runtime
            
            ```
            sudo apt-get remove dotnet-dev-1.0.0-*
            ```

3. Install Mono (Required by KoreBuild)

    ```
    sudo apt-get install mono-complete
    //or
    sudo apt-get install mono-devel //faster
    ```

4. Install Visual Studio Code
    
    If you want to be able to edit/run/debug Orchard 2 this is the IDE that you need.

    ```
    curl -O https://az764295.vo.msecnd.net/stable/fa6d0f03813dfb9df4589c30121e9fcffa8a8ec8/vscode-amd64.deb
    sudo dpkg -i vscode-amd64.deb
    ```

5. Get Orchard from Github repository

    ```
    sudo apt-get install git
    git clone https://github.com/OrchardCMS/Orchard2.git

    cd Orchard2
    sh build.sh //really important to not do "sudo" else we will make the nuget packages to be accessible by only sudo wich will cause problems with the Omnisharp installer later on.
    ```

6. Install Omnisharp

    Download with browser
    https://github.com/OmniSharp/omnisharp-vscode/releases/download/v1.0.3-rc2/csharp-1.0.3-rc2.vsix

    ```
    code //opens vs code from command shell
    ```

    Open the .vsix file with VS Code. It will add C# syntax highlighting and a debugger.
    
###Running in Docker

    ```
    cd /orchard2-root-folder //alternatively where the Dockerfile is ...
    sudo docker build -t orchard2 .
    ```

###Contributing

We currently follow the these [engineering guidelines](https://github.com/OrchardCMS/Orchard2/wiki/Engineering-Guidelines).
