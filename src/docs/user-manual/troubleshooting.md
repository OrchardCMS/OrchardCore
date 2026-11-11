---
title: Troubleshooting
description: Answers to common problems by symptom, such as a missing menu, an access denied page, a page not found after publishing, changes that don't show, rejected uploads, emails not sent, a locked account and empty search results.
technical_manual:
  - reference/modules/Diagnostics/README.md
  - reference/modules/Roles/README.md
  - guides/lockout-configuration/README.md
  - reference/modules/Email/README.md
  - reference/modules/Media/README.md
---

# Troubleshooting

Find your symptom below and work through the checks in order. Most problems come from a feature that's off, a missing permission, or content that was saved but not published. When a check needs an administrator or your IT team, it says so.

## I don't see a menu or a button

1. **Check the page for that task in this manual.** The table at the top names the **Menu**, the **Permission** and the **Feature** it needs. A menu is hidden while its feature is off or your role lacks its permission.
2. **Ask an administrator** to give you the permission or turn the feature on (in **Tools > Features**). See [Roles and permissions](getting-started/roles-and-permissions.md) for what to ask.
3. **Some buttons depend on the content type or your permissions:**
    - **Publish** shows only if you may publish that item. Without it you see **Save Draft** only.
    - **Save Draft** shows only on **Draftable** content types. See [Content types](content/content-types.md).
    - **Publish Later** shows only on types with the **Publish Later** part, and only if you may publish.
    - **Set as homepage** shows only on types whose **Autoroute** settings allow it, and only with the permission *Set homepage.*
    - **Localizations** shows only on types with the **Localization** part, once the item has been saved.
4. **Content Items is missing.** It needs *List content items*, or a per-type permission such as *List Page content items*. The built-in **Author** and **Contributor** roles don't have it.
5. **The New menu is missing.** It's off by default; administrators turn it on with **Display New menu** in **Settings > Admin**. You can always create content from **Content > Content Items > New**.

## I get "You do not have access to this resource."

You're signed in, but none of your roles has the permission this page needs.

1. Check which page you opened, and look up its permission in this manual.
2. Ask an administrator to add the permission to one of your roles. See [Roles](users/roles.md).
3. To use the admin at all, a role needs *Access admin panel*.

If you aren't signed in, the site sends you to the sign-in page instead; sign in and try again.

## I can't publish

1. **There's no Publish button.** Your roles can't publish this item: you may have *Edit own content* but not *Publish or unpublish own content*, or the item belongs to someone else and you lack the *... for others* permission. Save a draft and ask an editor to publish it. See [Set up content approval](use-cases/content-approval.md).
2. **You click Publish but the editor comes back.** A field has an error, shown in red next to it, such as a required field left empty. Fix it and publish again.
3. **The item was published but isn't on the site.** See the next two sections.

## The page isn't found after publishing

The site shows *The page could not be found.*

1. **Check it's published.** In **Content > Content Items**, the item must show **Published**. A draft, or a scheduled item before its time, isn't visible.
2. **Check its address.** Open the item and look at **Permalink**. The address you typed must match it exactly. Blog posts' addresses start with the blog's address, such as `/blog/my-post`.
3. **Did the address change?** Editing **Permalink** changes the address; old links stop working. Change it back, or ask IT about a redirect (see [URL rewriting](settings/url-rewriting.md)).
4. **No Permalink box at all?** The content type has no **Autoroute** part, so its items have no address of their own. See [URLs and the home page](content/urls-and-homepage.md).
5. **Is it the home page?** The home page shows at the site's root address.

## My changes don't show on the site

1. **Did you publish?** **Save Draft** keeps changes out of sight. Click **Publish**.
2. **Menus, taxonomies and widgets are content too.** Adding a menu item or a term saves its menu or taxonomy as a draft; open it and click **Publish**.
3. **A widget doesn't show?** Check its layer's rule (a layer with no rule shows nowhere), that the widget is published, and that the theme displays its zone. See [Widgets and layers](design/widgets-and-layers.md).
4. **Reload without the browser's cache** (Ctrl+F5, or Cmd+Shift+R on a Mac), or try a private window.
5. **Signed in vs visitor.** Some content shows only to some roles. Check in a private window, as a visitor.
6. **Still old after a few minutes?** Parts of pages can be cached by the site. Ask IT if a change never appears.

To see a draft as visitors will once it's published, use **Preview**. See [Content preview](content/preview.md).

## A scheduled item wasn't published

1. Open the item: it must show the scheduled date, set with **Publish Later**.
2. Check the time zone: the date is in your profile's time zone, or the site's if you haven't set one.
3. Ask an administrator to check **Tools > Background Tasks**: the **Scheduled Content Items Publisher** task must be enabled. See [Background tasks](settings/background-tasks.md).

## My upload is rejected

| Message | Why | What to do |
| --- | --- | --- |
| *This file extension is not allowed: ...* | The file type isn't in the site's allowed extensions. | Use another format, or ask IT to allow it. |
| *This file exceeds the maximum upload size of ...* | The file is larger than the limit. | Make the file smaller (resize or compress images), or ask IT to raise the limit. |
| *Media extension is not allowed. Only media with '...' extensions are allowed.* | The field you're filling accepts only some file types. | Pick a file of an accepted type, or ask a site builder to change the field's settings. |
| *You are not authorized to perform this action on this file* (or *folder*) | You may not change that file or folder. | Ask for the media permissions you need. See [Media settings](media/media-settings.md). |

Administrators can see the limits in **Settings > Media > Options**: **Allowed file extensions**, **Restricted file extensions** (uploading those needs the permission *Upload media file extensions requiring additional permission*) and **Max file size**. These values come from the site's configuration, so only IT can change them.

## Emails aren't sent

1. **Is a provider set up?** Open **Settings > Communication > Email**. A provider must be enabled on its tab and picked in **Default provider**. *No email providers are currently enabled.* means an administrator must first turn on a provider feature, such as **SMTP Email Provider**.
2. **Send a test.** In **Tools > Testing > Email Test**, send an email to yourself. *Unable to send the message using the selected provider.* means the provider's details are wrong or its server refused; check them with IT.
3. **Check the spam folder** of the recipient.
4. **From a workflow?** Open **Design > Workflows**, click **Instances**, and open the run: if **Send Email** took its **Failed** outcome, the problem is the provider; if the workflow never ran, check it's published and enabled.
5. **Password reset or registration emails?** They use the same provider; fix it as above.

See [Email](settings/email.md).

## I'm locked out of my account

| Message | Why | What to do |
| --- | --- | --- |
| *Invalid login attempt.* | Wrong user name or password. | Check them (passwords are case-sensitive), or use **Forgot your password?**. |
| *The specified user is not allowed to sign in.* | Your account is disabled. | Ask an administrator to enable it. |
| *The account is locked out* | Too many wrong passwords in a row. | Wait a while and try again, or ask an administrator to unlock you. |

Administrators: in **Access Control > Users**, open the person's **Actions** menu and click **Unlock** (shown only for a
locked account), **Enable** for a disabled account, or **Disable two-factor authentication** when someone lost their authenticator device and recovery codes. How many attempts lock an account, and for how long, is set by IT. See [Users](users/users.md) and [Signing in](getting-started/signing-in.md).

## There's no "Forgot your password?" link

The link appears on the sign-in page only when both are true:

1. the feature **Users Reset Password** (`OrchardCore.Users.ResetPassword`) is on;
2. **Allow the users to reset their password** is ticked in **Settings > Security > Reset Password**.

Email must work too, because the reset link is sent by email. See [Login and registration](users/login-and-registration.md).

## Search returns nothing

1. **"No default search index has been configured."** Pick an index in **Settings > Search > Site Search**.
2. **Visitors are sent to the sign-in page.** The **Anonymous** role needs the permission *Query '...' Index* for the index (in the **Indexing** group of the role editor).
3. **Some items are missing.** Check that the index includes their content type (**Search > Indexes**, **Edit**), and that they're published.
4. **Still nothing?** In **Search > Indexes**, open the index's actions menu and click **Rebuild**, then wait a moment.

See [Add site search](use-cases/site-search.md) and [Site search](search/site-search.md).

## A workflow doesn't run

1. In **Design > Workflows**, open it. Changes count only once you click **Publish** in the designer.
2. On the designer's **Workflow** tab, **Enabled** must be ticked.
3. It needs a start activity (an event marked **Start**).
4. Click **Instances** on the workflows list to see each run; a faulted run shows where it stopped.

See [Workflows](forms-and-automation/workflows.md).

## A site shows the setup screen

A tenant that was created but never set up shows the setup screen. In **Multi-Tenancy > Tenants**, such tenants show **Uninitialized**, with a **Setup** button. See [Tenants](administration/tenants.md) and [Setting up a site](getting-started/setting-up-a-site.md).

If a site that was working suddenly shows the setup screen, don't run the setup: contact IT right away, as the site may have lost its connection to its database.

## The site shows an error page

*An error occurred while executing this request.* means something failed on the server.

1. Note the time, the address of the page and what you did right before.
2. If you changed something shortly before (a template in **Design > Templates**, a feature, a setting), undo it and try again.
3. Ask IT to check the site's logs for that time. Logs aren't visible in the admin.

*You have sent too many requests within a specified time frame.* means the site's rate limits blocked you for a moment. Wait and try again; administrators manage the limits in **Tools > Rate Limits**. See [Security](settings/security.md).

## Still stuck?

- The [Glossary](glossary.md) explains the admin's words.
- Your developer or IT team can find more in the Technical Manual, linked at the bottom of each page.
