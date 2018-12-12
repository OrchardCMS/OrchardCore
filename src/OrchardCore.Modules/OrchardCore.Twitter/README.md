# Twitter (OrchardCore.Twiter)

This module adds Twitter for Websites features to OrchardCore.

## Sign in with Twitter
Authenticates users with their Twitter Account. 

Create an app in the [Twitter Developer Platform](https://developer.twitter.com) and enable Sign in with Twitter.
In the app details, you must configure the Callback URL. The default url in OrchardCore is [tenant]/signin-twitter.

Configuration can be set through the _Twitter -> Sign in with Twitter_ settings menu in the admin dashboard.

Available settings are:

+ ConsumerKey: API key found in the keys and tokens tab of your twitter app.
+ ConsumerSecret: The API secret key of your twitter app.
+ CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.
If no value is provided, setup Callback URL in Twitter app to use the default path /signin-twitter.

*Users Registration*
+ If you want to enable new users to register to the site through their Twitter account, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
+ An existing user can link his account to his Twitter account through the External Logins link from User menu.



