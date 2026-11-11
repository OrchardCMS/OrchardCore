---
title: Users
description: Find people in the user list, add accounts, give roles, reset passwords, disable, unlock or delete accounts, and record what happens to them.
technical_manual:
  - reference/modules/Users/README.md
  - reference/modules/Users/TicketStore.md
  - topics/security/users.md
  - guides/password-configuration/README.md
  - guides/lockout-configuration/README.md
---

# Users

Everyone who signs in to the site has a user account. The user list is where administrators add people, give them roles, reset their passwords, and disable, unlock or delete accounts. The roles you give decide what each person can see and do, see [Roles](roles.md).

| | |
| --- | --- |
| **Menu** | **Access Control > Users** |
| **Permission** | *List all users* to open the list. *Edit any user*, *Delete any user*, *Assign any role to users* for the actions. *Manage security settings and all users* includes all of them except *Assign any role to users*. |
| **Feature** | **Users** (`OrchardCore.Users`) |


The dashboard also shows a **Manage users** card with a **Go to users** button.

## Find a user

Open **Access Control > Users**. Each row shows the user name and email address, a badge for each of the person's roles, and these status badges when they apply:

| Badge | Meaning |
| --- | --- |
| **Unconfirmed** | The person hasn't confirmed their email address. |
| **Locked** | The account is locked after too many wrong passwords. |
| **Disabled** | The account can't sign in. |

To narrow the list:

- Type in **Search** and press Enter. Plain words search the user name. You can also type terms such as `email:jane@example.com`, `role:Editor`, `status:Disabled` or `sort:Email`. The funnel button next to the box offers **Only enabled users**, **Only disabled users** and **Filter syntax**, which lists every term and the operators `AND`, `OR`, `NOT`, brackets and quotes.
- Use the menus above the list: **Role** (**Any role**, **Authenticated (no roles)** for people with no role, or a role name), **Show** (**All Users**, **Enabled Users**, **Disabled Users**) and **Sort** (**Name** or **Email**).

The list can also be shown as a grid with **User**, **Roles** and **Actions** columns. See [Finding your way](../getting-started/finding-your-way.md) for layouts and paging.

## Add a user

1. Open **Access Control > Users** and click **Add User** at the top right.
2. Fill in the form on the **Create User** page (see the table).
3. Click **Save**.

The list opens with *User created successfully.*

| Field | What it does |
| --- | --- |
| **User name** | Required. The name the person signs in with. It must be unique. Which characters are allowed is set by your IT team; by default only letters, digits and the symbols `-`, `.`, `_` and `+` are allowed. |
| **Email** | Required. Must be unique on the site. |
| **Phone Number** | Optional. Include the country code, for example *+1* for the United States. |
| **Password** | Required, typed twice (the second box is the confirmation). The eye button shows what you typed. **Generate password** fills both boxes with a password that follows the site's password rules, and **Copy password** copies it so you can pass it on. |
| **Roles** | Tick the roles the person needs. You only see the roles you are allowed to assign. |
| **Default User Culture** | The language of the admin for this person. Shown with the **User Localization** feature. |
| **Default Time Zone** | The time zone used for dates for this person. Shown with the **User Time Zone** feature. |
| **Notification Methods** | How the person receives notifications. See [Notifications](../forms-and-automation/notifications.md). |

Extra tabs appear when the site has [custom user settings](custom-user-settings.md).

A new account is enabled. Its email address is not confirmed, so it shows **Unconfirmed**. If the site requires confirmed emails (see [Login and registration](login-and-registration.md)), use **Confirm email** or **Send verification email** in the row's **Actions** menu before the person can sign in.

!!! note
    The password rules (length, digits, upper case and so on) are set by your IT team. When a password breaks a rule, the form says which one.

## Edit a user

1. In the user list, click **Edit** on the person's row.
2. Change the fields on the **Edit User** page.
3. Click **Save**.

The page has the same fields as **Create User**, without the password. Keep in mind:

- **User name**, **Email** and **Phone Number** are greyed out unless the matching option is on in **Settings > Security > User Login** (**Allow user to change their username**, **Allow user to change their email address**, **Allow user to change their phone number**). This applies to administrators editing someone else too.
- When you take roles away, the person's sessions end and they sign in again.
- You can't take the **Administrator** role away from the only enabled administrator. The site warns *Cannot remove Administrator role from the only enabled administrator.*

The editor has no "enabled", "email confirmed" or "locked" boxes. Those are actions in the list, described below.

To see an account without changing it, click **View** on the row. The **View User** page shows **User Info** (user name, email, phone number and status) and **Roles**. **View** needs *View user profiles* and is not shown on your own row.

## Use the Actions menu on a row

Click **Actions** on a row (or the actions cell in the grid). You see only the actions that apply to that account and that you are allowed to do:

| Action | What it does |
| --- | --- |
| **Change password** | Opens **Change Password**. Type the **New password** and **New password confirmation**, then click **Submit**. For your own account you also type your **Current password**. |
| **Disable** / **Enable** | Disable stops the account from signing in; the person sees *The specified user is not allowed to sign in.* Enable lets them in again. Both ask you to confirm. You can't disable the only enabled administrator. |
| **Unlock** | Shown when the account is locked. Clears the lock and the count of failed attempts so the person can sign in at once. |
| **Confirm email** | Shown when the email address is unconfirmed. Marks it as confirmed without sending anything. |
| **Send verification email** | Shown when the address is unconfirmed and the site requires confirmed emails. Sends the person a confirmation link. |
| **Disable two-factor authentication** | Shown when the person uses two-factor authentication and you have *Disable two-factor authentication for any user*. Use it when someone lost their phone and their recovery codes. |
| **Delete** | Deletes the account after you confirm. This can't be undone. Not shown on your own row. |

!!! tip "Approving people who registered"
    When the site requires approval of new accounts, a person who registers is created **Disabled**. Approve them with **Enable**.

## Change many users at once

1. Tick the boxes of the users, or the box at the top of the list to select the whole page.
2. Open **Actions** above the list and choose **Confirm email**, **Enable**, **Disable** or **Delete**.
3. Confirm.

A message lists each user that changed. Your own account is skipped for **Enable**, **Disable** and **Delete**, and users you aren't allowed to change are skipped too.

## Let someone manage only some users

Besides the "any user" permissions, the role editor has permissions per role, in the **Users** group:

| Permission | Allows |
| --- | --- |
| *List users in Editor role* | Seeing users who have that role. |
| *Edit users in Editor role* | Editing them, changing their password, enabling, disabling and unlocking them. |
| *Delete users in Editor role* | Deleting them. Not offered for the Administrator role. |
| *Assign Editor role to users* | Ticking that role on a user. Not offered for the Administrator role. |
| *Manage users in Editor role* | Doesn't include the four permissions above; tick the ones the person needs. |

Replace *Editor* with any role. A user who has no role at all can be handled by anyone who holds one of these permissions for any role. See [Roles](roles.md).

## Record what happens to user accounts

The **Users Audit Trail** feature (`OrchardCore.Users.AuditTrail`) writes user events to the audit trail: logged in, login failed, login account locked, created, updated, deleted, enabled, disabled and confirmed, plus registered, password reset and password recovered when those features are on. *Login failed* is off by default. You read the events in **Tools > Audit Trail** and choose which events are recorded in **Settings > Audit Trail**, see [Audit trail](../content/audit-trail.md).

| | |
| --- | --- |
| **Menu** | **Settings > Security > User Audit Trail** |
| **Permission** | *Manage Audit Trail settings for user events* (settings), *View Audit Trail events about users*, *View Audit Trail events about own user* (events) |
| **Feature** | **Users Audit Trail** (`OrchardCore.Users.AuditTrail`), which needs **Audit Trail** |

Each event can keep a copy of the account as it was (the "User Snapshot"). The **Audit Trail User Event Settings** page, under **User Properties to Store**, lists every property of an account, such as `Email`, `PhoneNumber` or `RoleNames`. For each one, choose:

| Choice | What is kept |
| --- | --- |
| (empty) | Nothing. The property is left out of the snapshot. |
| **Store** | The value as it is. |
| **PartialAsteriskRedactor** | The value with every character except the first and the last replaced by `*`, for example `j******m`. Good for emails and names. |
| **ErasingRedactor** | An empty value: the property is listed but its content is erased. |

Click **Save**. Passwords, reset tokens and other secret tokens are never stored. Every signed-in user has *View Audit Trail events about own user* by default, but only roles with *View audit trail* (Administrator by default) can open **Tools > Audit Trail**.

## Let people choose their time zone and language

- **User Time Zone** (`OrchardCore.Users.TimeZone`) adds **Default Time Zone** to the user editor and profile. Dates in the admin are shown and edited in that zone. **Use site's time zone** follows the site setting in [General settings](../settings/general.md).
- **User Localization** (`OrchardCore.Users.Localization`, needs **Localization**) adds **Default User Culture**. The list holds **Use site's culture** and the cultures added in **Settings > Localization > Cultures** (see [Localization](../content/localization.md)). The admin is shown in that culture for the person.

Both are optional for each person.

## Users Authentication Ticket Store

When someone signs in, the site normally keeps their sign-in details in a cookie in their browser. People with many roles or claims can end up with a cookie that is too large for some servers. The **Users Authentication Ticket Store** feature (`OrchardCore.Users.Authentication.CacheTicketStore`) keeps those details on the server instead and leaves only a small cookie in the browser. It has no screen. Turn it on only when your IT team asks; on a site that runs on several servers it needs a shared (distributed) cache. Everyone may need to sign in again after it is turned on or off.

## Related pages

- [Signing in](../getting-started/signing-in.md): what people see on the login page and their own profile.
- [Login and registration](login-and-registration.md): login options, self-registration, password reset and two-factor settings.
- [Custom user settings](custom-user-settings.md): extra fields on every user.
- [Import and export](../administration/import-and-export.md): copy users and roles between sites with deployment plans.
