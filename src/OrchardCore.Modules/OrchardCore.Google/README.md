# Google (OrchardCore.Google)

This module adds Google features to OrchardCore.

## Google Authentication
Authenticates users with their Google Account. 

Create a project in the [Google API Console](https://console.developers.google.com/projectselector/apis/library).

Add the Google+ API to your project. Navigate to Credentials and Create Credentials for your site.

In the 'Which API are you using' question select the Google+ API

In the 'Where will you be calling the API from' question select the Web server (e.g. node.js, Tomcat).

In the 'What data will you be accessing' question select 'User data'

Now click the 'What credentials do I need?' button and set the ClientID.
You must also set the authorized redirect URI to point to your Orchard instance. The default url in OrchardCore is [tenant]/signin-google

The next step is to parameterize the consent screen that will appear to the user.

Now you can download your credentials.

Configuration can be set through the _Google -> Google Authentication_ settings menu in the admin dashboard.

Available settings are:

+ ClientID: The client_id field value in the downloaded json file.
+ ClientSecret: The client_secret field value in the downloaded json file.
+ CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.
If no value is provided, setup Callback URL in Google API to use the default path /signin-google.

*Users Registration*
+ If you want to enable new users to register to the site through their Twitter account, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
+ An existing user can link his account to his Twitter account through the External Logins link from User menu.



