# Brochard

Brochard is the implementation of Orchard CMS in Asp.Net VNext (also known as DNX)

## Getting Started

First off, follow the instructions here https://github.com/aspnet/home in order to install DNVM. Next install Visual Studio 2015, or what ever you flavour of editor is.

Next you want to clone the Repo. 'git clone https://github.com/OrchardCMS/Brochard.git' and checkout the master branch.

Run the build.cmd file included in the repository to bootstrap DNX and build solution.

Next navigate to 'D:\Brochard\src\OrchardVNext.Web' or where ever your retrospective folder is on the command line in Administrative mode.

run.. 'dnx web' -> Hey you just kicked up the Orchard host.

Then in your browser, call the url... http://localhost:5001/setup/index

## Using Brochard

### Creating a host

When running Brochard, you need a client. The default implementation is to have a client talk to a host.

The client is any project that creates the host.

To create the host in a web project you would do

```c#
public class Startup {
    public IServiceProvider ConfigureServices(IServiceCollection services) {
        return services
        // AddHostSample is where the magic is done. This extension method lives in the Host (OrchardVNext.Hosting.Web)
            .AddHostSample()
            .BuildServiceProvider();
    }
}
```

The Host has a small wrapper


```c#
public static IServiceCollection AddHostSample([NotNull] this IServiceCollection services) {
    // This will setup all your core services for a host
    return services.AddHost(internalServices => {
        // The core of the host
        internalServices.AddHostCore();
        ///... All extra things you want registered so that you don't have to touch the core host.
    });
```

### Additional module locations

Additional locations for module discovery can be added in your host setup.

```c#
public static IServiceCollection AddHostSample([NotNull] this IServiceCollection services) {
    return services.AddHost(internalServices => {
        internalServices.AddHostCore();

        // Add folders the easy way
        internalServices.AddModuleFolder("~/Core/OrchardVNext.Core");
        internalServices.AddModuleFolder("~/Modules");

        // Add folders the move configurable way
        internalServices.Configure<ExtensionHarvestingOptions>(options => {
            var expander = new ModuleLocationExpander(
                DefaultExtensionTypes.Module,
                new[] { "~/Core/OrchardVNext.Core", "~/Modules" },
                "Module.txt"
                );

            options.ModuleLocationExpanders.Add(expander);
        });
    });
}
```
