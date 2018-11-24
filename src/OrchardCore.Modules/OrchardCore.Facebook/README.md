# OrchardCore.Facebook

## Facebook Module
`OrchardCore.Facebook` provides the following features 

- Core Components
- Facebook Login

Configuration can be set through the _Configuration -> Facebook_ settings menu in the admin dashboard.

## Core Components
Registers the Facebook App with site.

Available settings are:

+ AppId: Facebook application id.
+ AppSecret: The application secret.

Both settings are available in the [facebook for developers application's](https://developers.facebook.com/apps) page, under Basic Settings.

## Facebook Login
Authenticates users from Facebook. 
If the site allows to register new users, a local user is created and the Facebook login is linked.
If a local user with the same email is found, then the external login is linked to that account, after authenticating.

The Facebook Login Product should be enabled in the [facebook for developers page](https://developers.facebook.com/apps) for web apps, 
and a valid OAuth redirect URI must be set.

Available settings are:

+ CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.
If no value is provided, setup facebook app to use the default path /signin-facebook.

*Users Registration*_
+ If you want to enable new users to register to the site through their Facebook login, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
+ An existing user can link his account to his Facebook login through the External Logins link from User menu



