# Github (OrchardCore.Github)

This module adds Github Authentication to OrchardCore.

## Authenticate with Github

Authenticates users with their Github Account. 

Create an OAuth app in the [Github Developer applications](https://github.com/settings/developers).  
In the app details, you must configure the Authorization Callback URL. The default url in OrchardCore is [tenant]/signin-github.

Configuration can be set through the _Github -> Authenticate with Github_ settings menu in the admin dashboard.

Available settings are:

- ClientID: Client ID found in your Github app.
- Client Secret: The secret key of your Github app.
- CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.  
If no value is provided, setup Authorization callback URL in Github app to use the default path /signin-github.

*Users Registration*

- If you want to enable new users to register to the site through their Github account, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
- An existing user can link his account to his Github account through the External Logins link from User menu.
