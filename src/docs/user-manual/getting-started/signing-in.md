---
title: Sign in and manage your profile
description: Sign in to the admin, reset a forgotten password, use two-factor authentication, sign out, and keep your own profile, password and email up to date.
technical_manual:
  - topics/security/login.md
  - reference/modules/Users/README.md
  - guides/password-configuration/README.md
  - guides/lockout-configuration/README.md
---

# Sign in and manage your profile

Everyone who works in the admin has a user account. This page covers what you see as a person signing in: the login page, a forgotten password, creating an account when the site allows it, the second step of two-factor authentication, signing out, and the pages where you look after your own account.

What you see depends on how your administrator set the site up. A link or a button described here is missing when the matching option or feature is off. The settings behind each one are described in [Login and registration](../users/login-and-registration.md).

| | |
| --- | --- |
| **Menu** | The login page, at `/Login` on your site, and the user menu at the top right of the admin |
| **Permission** | None to sign in. *Edit own user information* to edit your profile |
| **Feature** | **Users** (`OrchardCore.Users`) |

## Sign in

1. Open the login page.
2. Type your user name or your email address in **Username or email address**.
3. Type your password in **Password**.
4. Tick **Remember me** if you want to stay signed in after you close the browser.
5. Click **Log in**.

Going to any admin page while you are signed out also takes you to the login page. The eye button next to **Password** shows or hides what you typed. **Remember me** is shown only when the administrator allows it.

The admin opens, or the page you were trying to reach. When something goes wrong, a message appears above the form:

| Message | What it means |
| --- | --- |
| **Invalid login attempt.** | The user name, email or password is wrong. |
| **The account is locked out** | Too many wrong passwords in a row. See [If your account is locked](#if-your-account-is-locked). |
| **The specified user is not allowed to sign in.** | Your account is disabled. Ask an administrator to enable it. |
| **Local login is disabled.** | The site accepts only external services (see below), not passwords. |

Instead of the form you may see one of these pages:

- **Confirm your email**: the site requires a confirmed email address before you can sign in. Open the confirmation email you received when the account was created, or click **Send confirmation email** to get a new one. You can ask for a new email only once a minute.
- **User Registration Pending**: your account was created but an administrator must approve it first.
- **Two-factor authentication**: your account uses a second step. See [Complete two-factor sign-in](#complete-two-factor-sign-in).

### Sign in with another service

When the site is connected to a service such as Microsoft, Google, Facebook, GitHub or X, the login page shows a second column, **Use another service to log in**, with one button per service.

1. Click the service's button.
2. Sign in on the service's own page.

You come back to the site signed in.

The first time, the site may ask you to finish creating an account (**Create a new account.**) or to link the service to an account you already have (**Link your account.**, where you type your local password once). See [External sign-in](../users/external-sign-in.md).

If the administrator turned off password sign-in, only the service buttons are shown.

## Reset a forgotten password

The **Forgot your password?** link appears under the login form when the administrator allows people to reset their own password.

1. On the login page, click **Forgot your password?**.
2. On the **Forgot password?** page, type your user name or email in **Username or email address**.
3. Click **Submit**. The site shows **Please check your email to reset your password.**
4. Open the email.
5. Click the reset link.
6. On the **Reset password** page, type your user name or email in **Username or email address**.
7. Type the new password in **New password** and in **New password confirmation**.
8. Click **Reset password**. The site confirms **Your password has been reset.**
9. Click **Click here to log in**.
10. Sign in with the new password.

!!! note
    If the site requires confirmed email addresses and yours is not confirmed, the site answers *Before you can reset your password, you need to verify your email address.* Confirm your email first. If no email arrives at all, the site's email settings may not be set up: tell your administrator.

## Create an account

Some sites let visitors create their own account. The login page then shows **Register as a new user**.

1. Click **Register as a new user**.
2. On the **Register a new account** page, type a **Username** and your **Email address**.
3. Type a password in **Password** and in **Confirm password**.
4. Click **Register**.

What happens next depends on the site:

- You are signed in right away.
- The page **Email confirmation sent** asks you to click the link in the email you received. The link opens **Email confirmation**: *Thank you for confirming your email.* You can then sign in.
- The page **User Registration Pending** tells you an administrator must approve the account before you can use it.

A new account gets no role, so it usually can't open the admin until an administrator gives it one. See [Roles and permissions](roles-and-permissions.md).

## Complete two-factor sign-in

With two-factor authentication, signing in takes your password plus a one-time code. After you click **Log in**, the page **Two-factor authentication** says *Your login is protected with a second authentication method.*

1. Get a code, depending on your method:
    - **Authenticator app**: open the app on your phone and read the code for this site.
    - **Email**: click **click here** in *Please click here to receive a one-time verification code via email.*, then open the email.
    - **SMS**: click **click here** in the SMS line, then read the text message on your phone.
2. Type the code in **Verification code**.
3. Tick **Remember this device** if you don't want to be asked for a code on this browser for a while.
4. Click **Log in**.

**Remember this device** appears only when the administrator allows it. If you set up more than one method, **Having trouble? Sign in another way.** switches to the next one.

### Sign in with a recovery code

When you have lost your phone or can't receive codes:

1. On the **Two-factor authentication** page, click **log in with a recovery code**.
2. On **Recovery code verification**, type one of your saved recovery codes in **Recovery Code**.
3. Click **Log in**.

Each recovery code works once. After signing in this way, set up your method again and create new codes (see [Manage your two-factor authentication](#manage-your-two-factor-authentication)).

## If your account is locked

After several wrong passwords in a row, the site locks the account for a while to stop someone guessing it. The number of attempts and the length of the lock are set by your IT team. Wait and try again, or ask an administrator to unlock your account. They do it from the user list, see [Users](../users/users.md).

## Sign out

1. Click your user icon at the top right of the admin.
2. Click **Log off**.

The user menu shows **Signed in as** and your user name above **Log off**.

## Edit your profile

1. Open the user menu at the top right.
2. Click **Profile**.
3. Change the fields you need on the **Edit User** page.
4. Click **Save**.

| Field | What it does |
| --- | --- |
| **User name** | The name you sign in with. Greyed out unless the administrator lets users change it. |
| **Email** | Your email address, used for password resets and notifications. Greyed out unless the administrator lets users change it. |
| **Phone Number** | Optional. Must include the country code, for example *+1* for the United States. The icon next to it shows whether the number is verified. Greyed out unless the administrator lets users change it. |
| **Roles** | Shown only to people who may assign roles. You can't give yourself roles otherwise. |
| **Default User Culture** | The language and regional format the admin uses for you. **Use site's culture** follows the site. Shown when the **User Localization** feature is on. |
| **Default Time Zone** | The time zone used to show and edit dates and times for you. **Use site's time zone** follows the site. Shown when the **User Time Zone** feature is on. |
| **Notification Methods** | How you receive notifications, in order of preference. See [Notifications](../forms-and-automation/notifications.md). |

Extra tabs can appear on this page when the site has [custom user settings](../users/custom-user-settings.md).

If **Profile** opens a read-only page, your role can view profiles but not edit your own. Ask for the *Edit own user information* permission.

## Change your password

1. Open the user menu at the top right.
2. Click **Change password**.
3. Type your **Current password**.
4. Type the **New password**.
5. Type it again in **New password confirmation**.
6. Click **Submit** (on the site's own pages, the button is **Change password**).

The site shows *The password has been changed successfully.* If the new password breaks one of the site's password rules, the message under the box says which rule. **Change password** is not in the menu when the site doesn't use passwords.

## Change your email address

The user menu shows **Change Email** when the administrator allows it and the **Users Change Email** feature (`OrchardCore.Users.ChangeEmail`) is on.

1. Open the user menu.
2. Click **Change Email**.
3. Type the new address in **Email**.
4. Click **Submit**.

The page **Email changed** confirms the change. The site refuses an address that is already your current one or that another user has.

## Manage your two-factor authentication

Open the user menu and click **Security**. The **Two-Factor Authentication** page shows whether two-factor authentication is on, and lists the methods the site offers under **Two-factor methods**.

### Set up an authenticator app

1. Next to **Authenticator app**, click **Add**.
2. Install an authenticator app on your phone, such as Microsoft Authenticator or Google Authenticator.
3. Scan the QR code with the app, or type the key shown on the page into the app.
4. Type the code the app shows in **Verification code**.
5. Click **Verify**.

The first time, the site turns two-factor authentication on and shows your **Recovery Codes**. Write them down or store them somewhere safe: they are the only way in if you lose your phone.

Later, **Set** shows the QR code again (to add another device), and **Reset** gives you a new key, after which the old app entry stops working until you set it up again.

### Use your email or phone

**Email authentication** shows **Verified** when your email address is confirmed. Otherwise:

1. Click **Verify Email**.
2. Open the email you receive.

To verify your phone for **SMS authentication**:

1. Click **Verify Phone Number**.
2. Check or type your number in **Phone Number**, with the country code.
3. Click **Request Code**.
4. Type the code you received in **Verification code**.
5. Click **Verify**.

Once a method is verified, click **Enable** at the top of the card to turn two-factor authentication on, if it isn't already.

### Other options on the page

| Option | What it does |
| --- | --- |
| **Preferred Two-Factor Method** | Shown when you have more than one method. Choose the one the site asks for first, then click **Update**. |
| **Forget this device** | Shown when this browser is remembered. After clicking it, the next sign-in asks for a code again. |
| **Reset codes** | Under **Recovery options**. Creates a new set of recovery codes; the old ones stop working. A warning appears when you have three or fewer codes left. |
| **Disable two-factor authentication** | Turns the second step off. Not shown when the site requires two-factor authentication for you; the button then reads **Enabled**. |

!!! warning "Required by your site"
    If the site requires two-factor authentication for everyone or for one of your roles, you are sent to this page after signing in, with the message *Two-factor authentication must be enabled before proceeding.*, until you set up a method.

### Manage linked services

When the site offers external services, the user menu has **External Logins**. That page lists the services linked to your account under **Registered Logins**, with **Remove** next to each, and offers buttons to link another one. **Remove** is shown only when you would still have a way to sign in: a local password or another linked service.
