---
title: Login and registration
description: Choose how people sign in, whether visitors can create accounts, how passwords are reset and email addresses changed, and how two-factor authentication works on your site.
technical_manual:
  - topics/security/login.md
  - topics/security/registration.md
  - reference/modules/Users/README.md
  - reference/modules/ReCaptcha/README.md
  - guides/password-configuration/README.md
  - guides/lockout-configuration/README.md
---

# Login and registration

These settings decide what people meet on the login page: whether they can stay signed in, create their own account, reset a forgotten password or change their email address, and whether they need a second step with a one-time code. What people see as a result is described in [Signing in](../getting-started/signing-in.md).

| | |
| --- | --- |
| **Menu** | **Settings > Security > User Login**, **Registration**, **Reset Password**, **Change Email** |
| **Permission** | *Manage security settings and all users* |
| **Feature** | **Users** (`OrchardCore.Users`), plus the features named in each section |

!!! warning "Test before you sign out"
    A wrong setting here can stop people, including you, from signing in. After a change, try signing in from a private browser window before you close your own session.

## Set the login options

1. Open **Settings > Security > User Login**.
2. Change the options on the **General** tab (see the table).
3. Click **Save**.

The page has more tabs when features add them: **Two-Factor Authentication** (see [Set up two-factor authentication](#set-up-two-factor-authentication)) and **External Login** (see [External sign-in](external-sign-in.md)).

| Option | What it does |
| --- | --- |
| **Allow user to be remembered during login** | Shows the **Remember me** box on the login page, so people can stay signed in after closing the browser. |
| **Use persistent authentication cookies by default** | When **Remember me** is shown, this is whether the box starts ticked. When **Remember me** is hidden, this decides for everyone whether they stay signed in after closing the browser. |
| **Allow user to change their username** | Lets people change their **User name** on their profile. Also unlocks the box when an administrator edits someone else's account. |
| **Allow user to change their email address** | The same for **Email**. |
| **Allow user to change their phone number** | The same for **Phone Number**. It also lets people change the number they use for SMS two-factor codes. |
| **Use site theme for login page** | Shows the login page in the site's theme instead of the admin theme. Requires an active site theme. |
| **Disable local password login.** | Hides the user name and password form: people can only sign in with an external service. **Change password** disappears from the user menu. Before you tick it, make sure at least one administrator has linked an external account, or nobody can get into the admin. |

## Let visitors create an account

Self-registration is open as soon as the **Users Registration** feature (`OrchardCore.Users.Registration`) is on. The login page then shows **Register as a new user**. There is no separate on/off switch: to stop registrations, turn the feature off in **Tools > Features**. The feature needs **Email**, and the site's email must be set up so confirmation emails can go out, see [Email](../settings/email.md).

1. Open **Settings > Security > Registration**.
2. Set the options on the **General** tab.
3. Click **Save**.

| Option | What it does |
| --- | --- |
| **Users must verify their email address** | New accounts must click the link in a confirmation email before they can sign in. Ticking it shows a warning: *The users whose email address has not been confirmed will not be able to login or reset their password.* This applies to every account with an unconfirmed address, including accounts you create in the admin. |
| **Users must be approved before they can log in** | New accounts are created disabled. The person sees **User Registration Pending** until an administrator enables the account in [Users](users.md). |
| **Use site theme for registration page** | Shows the registration page in the site's theme. Requires an active site theme. |

People who register get no role. Give them the roles they need in [Users](users.md), or add permissions to the **Authenticated** role if every account should have them.

!!! note
    Which characters a user name may contain, and whether every account needs a different email address, are set by your IT team, not on these screens. By default user names allow letters, digits and `-._+`, and email addresses must be unique.

The **External Authentication** tab on the same page holds the rules for people who register by signing in with an external service. See [External sign-in](external-sign-in.md).

## Let people reset a forgotten password

| | |
| --- | --- |
| **Menu** | **Settings > Security > Reset Password** |
| **Feature** | **Users Reset Password** (`OrchardCore.Users.ResetPassword`), which needs **Email** |

1. Open **Settings > Security > Reset Password**.
2. Tick **Allow the users to reset their password**. An email with a reset link is sent to the person's address when they ask for it.
3. Optionally tick **Use site theme for password reset page**.
4. Click **Save**.

The login page then shows **Forgot your password?**. When the option is off, the reset pages are not available at all.

## Let people change their email address

| | |
| --- | --- |
| **Menu** | **Settings > Security > Change Email** |
| **Feature** | **Users Change Email** (`OrchardCore.Users.ChangeEmail`) |

Tick **Allow the users to change their email** and click **Save**. Each person's user menu then has **Change Email**, which changes the address right away. This is separate from **Allow user to change their email address** on the User Login page, which unlocks the **Email** box on the profile.

## Set up two-factor authentication

Two-factor authentication asks for a one-time code after the password. Turn on at least one method in **Tools > Features**:

| Feature | Method |
| --- | --- |
| **Two-Factor Authenticator App Method** (`OrchardCore.Users.2FA.AuthenticatorApp`) | Codes from an authenticator app on the person's phone. |
| **Two-Factor Email Method** (`OrchardCore.Users.2FA.Email`) | Codes sent by email. Needs **Email** set up, see [Email](../settings/email.md). |
| **Two-Factor SMS Method** (`OrchardCore.Users.2FA.Sms`) | Codes sent by text message. Needs **SMS** set up, see [SMS](../settings/sms.md). |

Turning one on also turns on **Two-Factor Authentication Services** (`OrchardCore.Users.2FA`), which adds **Security** to everyone's user menu and the **Two-Factor Authentication** tab to **Settings > Security > User Login**.

Each person then sets up their own method from **Security** in the user menu, see [Signing in](../getting-started/signing-in.md#manage-your-two-factor-authentication).

### Two-Factor Authentication tab

1. Open **Settings > Security > User Login** and the **Two-Factor Authentication** tab.
2. Set the options.
3. Click **Save**.

| Option | What it does |
| --- | --- |
| **Allow users to remember client** | Shows **Remember this device** when entering a code, so the person isn't asked again on that browser for a while. The hint shows how long. |
| **Require two-factor authentication** | Everyone must use two-factor authentication. People who haven't set it up are sent to their **Two-Factor Authentication** page after signing in and can't use the site until they add a method. They also can't turn it off. |
| **Number of recovery codes to generate** | How many one-time recovery codes each person gets. Must be at least 1. The default is 5. |
| **Use site theme for two-factor authentication pages** | Shows those pages in the site's theme. Requires an active site theme. |
| **Require two-factor authentication specific roles** | Shown when the **Roles** feature is on. Tick it, then tick the roles under *Select the roles to require two-factor authentication for.* People with one of those roles must set up two-factor authentication, as above. You must select at least one role. |

When the matching method is on, the tab also has:

| Section | Option | What it does |
| --- | --- | --- |
| **Email Authenticator Settings** | **Email Subject**, **Email Body** | The email that carries the code. Liquid is supported; `{{ Code }}` is replaced by the code. The default subject is *Your verification code*. |
| **Authenticator App Settings** | **Show email address in the app** | The entry in the person's app shows their email address instead of their user name. |
| **SMS Authenticator Settings** | **Message Body** | The text message that carries the code. Liquid is supported; include `{{ Code }}`. |

!!! tip
    Ask everyone to keep their recovery codes. If someone loses their phone and their codes, an administrator can use **Disable two-factor authentication** in the person's **Actions** menu in [Users](users.md), so they can sign in with their password and set it up again.

## Locked accounts and password rules

After several wrong passwords in a row, the site locks the account for a while. The person sees *The account is locked out*. The number of attempts, the length of the lock, and the password rules (minimum length, digits, upper case and so on) are set by your IT team, not in the admin. To let someone in before the lock ends, use **Unlock** in [Users](users.md).

When the **Rate Limits** feature is on, the login, registration, password and two-factor pages can also limit how many attempts come from one place in a given time, see [Security](../settings/security.md).

## Protect the forms with reCAPTCHA

The **ReCaptcha Users** feature (`OrchardCore.ReCaptcha.Users`) adds a reCAPTCHA check to the **Log in**, **Register a new account**, **Forgot password?** and **Reset password** forms, to stop automated sign-in and sign-up attempts. It turns on **ReCaptcha** too.

The check appears only after the site key and secret key are filled in on **Settings > Security > reCaptcha**, see [Security](../settings/security.md). People who sign in with an external service aren't asked to solve it.

## The emails people receive

The site sends a *Confirm your account* email for email confirmation and a reset link for forgotten passwords. Both go through the site's email settings. Their wording is part of the site's templates; ask your developer if it needs to change.
