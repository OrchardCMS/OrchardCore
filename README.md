[![brochard MyGet Build Status](https://www.myget.org/BuildSource/Badge/brochard?identifier=098718e3-f53d-4bcd-b29e-cb9da86823c0)](https://www.myget.org/)

# Orchard 2

Orchard 2 is the implementation of [Orchard CMS](https://github.com/OrchardCMS/Orchard) in [ASP.NET vNext](http://www.asp.net/vnext) (also known as DNX). You can check out the [Orchard 2 presentation from the last Orchard Harvest](https://www.youtube.com/watch?v=TK6a_HfD0O8) to get an introductory overview of its features and goals.

[![Join the chat at https://gitter.im/OrchardCMS/Orchard2](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OrchardCMS/Orchard2?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Orchard CMS

Orchard is a free, [open source](https://github.com/OrchardCMS/Orchard), community-focused Content Management System built on the ASP.NET MVC platform.

## Getting Started

- First off, [follow the instructions here](https://github.com/aspnet/home) in order to install DNVM, then install Visual Studio 2015, or what ever you flavour of editor is.
- Next you want to clone the repository using the command `git clone https://github.com/OrchardCMS/Orchard2.git` and checkout the `master` branch.
- Run the `build.cmd` file included in the repository to bootstrap DNX and build the solution.
- Next navigate to "D:\Orchard2\src\Orchard.Web" or where ever your retrospective folder is on the command line in Administrator mode.

### Using Kestrel

- Call `dnx web`.
- Then open the `http://localhost:5001` URL in your browser.

### Using Console

- Call `dnx run`.
- From here you can now execute commands in a similar fashion as before.

## Using Orchard 2

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

###Testing

We currently use XUnit to do unit testing.

###Contributing

We currently follow the these [engineering guidelines](https://github.com/OrchardCMS/Orchard2/wiki/Engineering-Guidelines).