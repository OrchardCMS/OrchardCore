# Users (OrchardCore.Users)

The Users module enables authentication UI and user management.

## Custom Paths

If you want to specify custom paths to login page, logoff, etc...

Below listed the configurable paths with their default values.  Specify the one you want to configure in appsettings.json:

``` json
  "OrchardCore": {
    "OrchardCore_Admin": {
      "LoginPath": "Login",
      "LogoffPath": "/Users/LogOff",
      "ChangePasswordUrl": "ChangePassword"
    }
  }
```
