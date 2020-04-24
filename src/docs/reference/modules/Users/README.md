# Users (OrchardCore.Users)

The Users module enables authentication UI and user management.

## Custom Paths

If you want to specify custom paths to access the authentication related urls, you can change ithem by using this option in the appsettings.json:

``` json
  "OrchardCore": {
    "OrchardCore_Users": {
      "LoginPath": "Login",
      "LogoffPath": "LogOff",
      "ChangePasswordUrl": "ChangePassword"
    }
  }
```
