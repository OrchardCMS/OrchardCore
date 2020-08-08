# How to change the password requirements

The password restrictions are set with the settings configured in ASP.NET Identity.  
Those options are used to define the required password strength when a user password is set.    
You can configure these requirements in order to specify properties like the minimal password length or if the process is expecting digits, uppercase or non alphanumeric characters.

## Configure password settings in ConfigureServices

The easiest way is to change these settings is to add this code in the `ConfigureServices` method in your `Startup` class:

```cs
 services.Configure<IdentityOptions>(options =>
 {
   options.Password.RequireDigit = false;
   options.Password.RequireLowercase = true;
   options.Password.RequireUppercase = true;
   options.Password.RequireNonAlphanumeric = false;
   options.Password.RequiredUniqueChars = 3;
   options.Password.RequiredLength = 6;
 });
```

!!! note
    This is just an example. You need to choose values that are compliant with your security requirements.

The documentation about the PasswordOptions in ASP.NET Core Identity is available here:  
https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordoptions?view=aspnetcore-3.1

## Reading configuration from an external config file

A better way to achieve this is to read configuration from an external file.  
This will allow you to specify different settings depending on the environment and transform the configuration section when you deploy it.

To do this, add a `Configuration` property in the `Startup` class:

```cs
public IConfiguration Configuration { get; }
```

In order to initialize the `Configuration`, set the property in the constructor like this:

```cs
 public Startup(IHostingEnvironment env)
 {
   var builder = new ConfigurationBuilder()
       .SetBasePath(env.ContentRootPath)
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

   Configuration = builder.Build();
 }
```

Then, replace the hard coded configuration with this code that reads it from the configuration file:

```cs
services.Configure<IdentityOptions>(options =>
 {
    Configuration.GetSection("IdentityOptions").Bind(options);
 });
```
 
Finally, create a file called `appsettings.json` with this configuration in json:

```json
{
 "IdentityOptions": {
   "Password": {
     "RequireDigit": false,
     "RequireLowercase": true,
     "RequireUppercase": true,
     "RequireNonAlphanumeric": false,
     "RequiredUniqueChars": 3,
     "RequiredLength": 6
   }
 }
}
```

## Summary

You just learnt how to configure the password settings from an `appsettings.json` file.

# Original blog post

http://www.ideliverable.com/blog/how-to-change-the-password-requirements-for-orchard-core-sites
