# How to change the Lockout configuration

The Lockout settings are set with the settings configured in ASP.NET Identity.  
Those options are used to define, for example, the TimeSpan a user is locked out for when a lockout occurs or the number of failed access attempts allowed before a user is locked out.   

The documentation about the LockoutOptions in ASP.NET Core Identity is available here:  
https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.lockoutoptions

## Reading configuration from an external config file

A better way to achieve this is to read configuration from configuration sources as settings files, environment variables, command-line arguments. For a complete list of the default configuration sources used by the default configuration providers, and how you can customize it, see the related ASP.NET Core documentation:  
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/

Using the `appsettings.{Environment}.json` settings files, for example `appsettings.json`, `appsettings.Production.json` and `appsettings.Development.json`, will allow you to specify different settings depending on the environment and transform the configuration section when you deploy it.

To do this, add a `Configuration` property in the `Startup` class:

```csharp
public IConfiguration Configuration { get; }
```

In order to initialize the `Configuration`, set the property in the constructor like this:

```csharp
public Startup(IConfiguration configuration)
{
    Configuration = configuration;
}
```

Then, bind configuration with this code that reads it from the configuration file:

```csharp
services.Configure<IdentityOptions>(options =>
{
    Configuration.GetSection("IdentityOptions").Bind(options);
});
```
 
Finally, create a file called `appsettings.json` with this configuration in json:

```json
{  
  "IdentityOptions": {
    "Lockout": {
      "AllowedForNewUsers": true,
      "DefaultLockoutTimeSpan ": "00:05:00",
      "MaxFailedAccessAttempts ": 5
    }
  }
}
```

## Summary

You just learned how to configure the Lockout settings from an `appsettings.json` file.
