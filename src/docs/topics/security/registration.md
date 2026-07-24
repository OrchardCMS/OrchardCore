# Registration

By default, only administrators create accounts. To let visitors sign up themselves, enable the **Users Registration** feature, which adds a `/Register` page and its settings under `Security → Settings → User Registration`:

- Require new users to verify their email address before they can log in (requires a configured [Email](../../reference/modules/Email/README.md) provider).
- Moderate registrations, so an administrator has to approve each new account.
- Render the registration pages with the site theme.

When external login providers are enabled (see [Login](login.md)), separate [external registration settings](../../reference/modules/Users/README.md#external-authentication-settings) control whether an account is created automatically for users who authenticate with an external provider, and which information (username, email, password) is asked or generated.

Registration settings can also be applied from a recipe: see the [Registration Settings recipe step](../../reference/modules/Users/README.md#registration-settings).
