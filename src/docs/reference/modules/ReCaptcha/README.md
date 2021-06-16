# ReCaptcha (`OrchardCore.ReCaptcha`)

The OrchardCore.ReCaptcha module can be used to prevent robots from abusing your OrchardCore website.

There are four features in the module:

## Configuration
In order to activate the ReCaptcha feature, you have to create an account with Google and enter the secret and site key in the Admin section.
You can sign up here: https://developers.google.com/recaptcha/

### Users protection
You can enable this feature in the admin section and your login pages will be protected against robots.
The feature will use the IP address of the request to count the number of login attempts. 
When the threshold for login attempts are broken, a captcha is shown on the login page preventing robots from making any further requests.

### Forms
You can add protection from robots to forms by including the recaptcha field when you design a form.

### Workflow
You can add a validate ReCaptcha task in your workflow.
You can use this to validate the captcha that you show on your OrchardCore.Forms form.

### Manual validation
You can decorate your controllers with [ValidateReCaptcha] attribute.
This attribute works in tandem with the <recaptcha /> HTML element, both need to be configured.
The standard mode is PreventAbuse, this will show the captcha when a robot is suspected.
The mode AlwaysShow, always shows the captcha on the page.
If you have a requirement to display the captcha in a specific language, you can use the language property to set it to desired language using either the culture string or the two letter ISO code of the language.

## Extending the module
If you have requirements that you have to protect against robots using another method than IP address,
you can create your own implementation of the IDetectRobots interface and it will join the robot detectors.

## Using with a form post with Content-Type = "application/json" from a javascript framework

The ReCaptcha api uses the data-callback attribute to return the token generated when validating the ReCaptcha widget. This allows to post that token from an Angular, Vue.js form post. If you want to validate the ReCaptcha from the Workflow task you will need to pass the token in the header of your request as "g-recaptcha-response".