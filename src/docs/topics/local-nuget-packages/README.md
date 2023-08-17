# Using a local copy of Orchard Core source code as nuget packages

In this article, we are going to create our own local nuget feed from our copy of the Orchard Core source code and add a new package source pointing to the local packages.  

## Create NuGet packages from your local source code.

For more information on dotnet pack see: <https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack>

- From the command line, go to the root folder of your fork/branch of the Orchard Core source code.
- Pack all of the NuGet packages to one output folder of your choice.  
Example: `dotnet pack -c Release -o c:\OrchardCoreNuget`

## Publish to your NuGet feed
For this example, we are going to use the Local Feed method.  For more information on this see: <https://docs.microsoft.com/en-us/nuget/hosting-packages/local-feeds>

- Create a folder for your NuGet Feed.  
For this example we are using `\\{YourServer}\NuGetServer`
- Add the NuGet packages to your local feed.  
Example: `nuget init c:\OrchardCoreNuget \\{YourServer}\NuGetServer`

## Update your project to use your NuGet feed

- Update your nuget.config file so it points to your local feed.  
https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file#packagesources
```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <packageSources>
        <clear />
        <add key="MyFeed" value="\\{YourServer}\NuGetServer" />
        <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
      </packageSources>
      <disabledPackageSources />
    </configuration>
```
- Make sure all of your projects are referencing the OrchardCore version in your local feed.