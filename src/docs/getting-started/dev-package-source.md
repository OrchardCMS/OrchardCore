# Add Dev package source

In this article, we are going to add a new package source pointing to the Dev packages.  
The Dev packages are built each time some code is committed on the `dev` branch, compared to the ones on NuGet, built from the `master` branch.  
They are the most up to date versions but not the most stable and can contain breaking changes.

!!! warning
    We do not suggest you to use the dev packages in production.

## Adding Orchard Core MyGet Feed to Visual Studio

In order to be able to use the __dev__ feed from Visual Studio, open the Tools menu under NuGet Package Manager --> Package Manager Settings.
The feed url is <https://www.myget.org/F/orchardcore-preview/api/v3/index.json>

![image](assets/addMyGetPackageSource.png)

