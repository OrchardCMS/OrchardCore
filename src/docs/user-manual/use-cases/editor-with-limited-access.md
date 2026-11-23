---
title: Give an editor limited access
description: Create a role that can edit only some content types, give it to a new user, and check what that person can and can't do in the admin.
technical_manual:
  - reference/modules/Roles/README.md
  - reference/modules/Users/README.md
  - reference/modules/Contents/README.md
---

# Give an editor limited access

Sometimes a person should look after one part of the site and nothing else: the marketing team edits pages, a volunteer writes events. Orchard Core does this with **roles**: a role is a set of **permissions**, and a user gets the permissions of all their roles. This walkthrough creates a *Page Editor* role that can create, edit and publish pages, but no other content, and gives it to a new user.

## Who it's for and what you get

For **administrators**.

When it's done:

- the **Page** content type has its own permissions;
- a **Page Editor** role that can list, create, edit, preview and publish pages, and upload images;
- a user with that role, who sees only the menus they need.

## Before you start

| You need | Where or who |
| --- | --- |
| The features **Roles** (`OrchardCore.Roles`), **Users** (`OrchardCore.Users`) and **Content Types** (`OrchardCore.ContentTypes`) | On by default with the built-in recipes |
| The permissions *Manage Roles*, *Manage security settings and all users* (or *Assign any role to users*) and *Edit content types.* | The **Administrator** role has them |

## Understand the two kinds of content permissions

The role editor has two kinds of content permissions:

- **General ones**, in the **Contents** group, such as *Edit content for others* or *Publish or unpublish own content*. They apply to **every** content type.
- **Per-type ones**, in a group named after each content type, such as **Page Content Type - Page**: *Edit Page*, *Edit Page for others*, *List Page content items*, and so on. They exist only for content types marked **Securable**.

To limit someone to some content types, give them **per-type** permissions only, never the general ones: *Edit content for others* would let them edit everything.

Each per-type permission comes in two forms: one for the person's own items (*Edit Page*) and one for items created by anyone (*Edit Page for others*). The second includes the first.

## Make the content type securable

The Blog recipe already marks **Page** (and **Blog** and its widgets) as securable. For another type, such as **Article**:

1. Open **Design > Content Definition > Content Types**.
2. Click **Edit** on the type.
3. Tick **Securable** (*Determines if this content type can have custom permissions.*).
4. Click **Save**.

A group for the type appears in the role editor. See [Content types](../content/content-types.md).

## Create the role

1. Open **Access Control > Roles**.
2. Click **Add Role**.
3. Type **Page Editor** in **Name**.
4. Type a **Description**, such as *Edits and publishes pages only*.
5. Click **Save & Configure** to open the role's permission editor.
6. Tick these permissions in the **Allow** column:

    | Group | Permission | Why |
    | --- | --- | --- |
    | **Admin** | *Access admin panel* | To open the admin at all. |
    | **Page Content Type - Page** | *List Page content items* | To see pages in **Content > Content Items**. Without a list permission, the **Content Items** menu is hidden. |
    | **Page Content Type - Page** | *Edit Page for others* | To create pages and edit every page, not only their own. |
    | **Page Content Type - Page** | *Publish or unpublish Page for others* | To publish. Leave it out if someone else must approve; see [Set up content approval](content-approval.md). |
    | **Page Content Type - Page** | *Preview Page by others* | To use **Preview**. |
    | **Page Content Type - Page** | *Delete Page for others* | Only if they may delete pages. |
    | **Media** | *Manage Own Media* | To upload images and choose them in pages. |

7. Click **Save** at the bottom.

The **Effective** column shows what the role really gets, including permissions implied by the ones you ticked: for example, *Edit Page for others* also gives *Edit Page* and *View Page by others*. Use the search box at the top to find a permission. See [Roles](../users/roles.md).

!!! note "Widgets inside pages"
    In the Blog recipe, the widgets used in a page's **Flow** (such as **Paragraph** and **Image**) are securable too. If the editor can't add or change a widget, also give the role *Edit ...* and *View ... by others* for those widget types, for example *Edit Paragraph for others*.

## Create the user

1. Open **Access Control > Users**.
2. Click **Add User**.
3. Type the **User name** and **Email**.
4. Type the **Password** and **Password Confirmation**.
5. Under **Roles**, tick **Page Editor** only.
6. Click **Save**.

Instead of typing a password, you can click **Generate password**, then **Copy password**. Give the person their user name and password in a safe way, and ask them to change the password when they first sign in. See [Users](../users/users.md).

## Check what they see

1. Open a private browser window.
2. Sign in as the new user.
3. Check the admin menu: it shows only what the role allows.
4. Create a page.
5. Click **Publish**: it works.
6. Open the content items list.
7. Sign out.
8. Close the private window.

The admin menu shows **Content > Content Items** (with **New** offering **Page**) and **Media**. **Design**, **Access Control**, **Settings** and **Tools** aren't there. On a Blog recipe site, the **Blog** and **Main Menu** links at the top still show: that custom admin menu sets no permission on them.

Other content types may still be listed in the content items list, because every signed-in user also has the **Authenticated** role, which can view all content by default. They show no **Edit** button, and opening their editor by address gives *You do not have access to this resource.*

## Tips

- Give permissions to roles, never one person at a time: when someone changes jobs, you change their roles.
- A user can have several roles; they get every permission of all of them. To take something away, check all their roles, including **Authenticated**, which every signed-in user has.
- To let someone manage the users of one role only, use the per-role permissions in the **Users** group, such as *Manage users in Page Editor role* and *Assign Page Editor role to users*.
- To hide some media folders from the role, see **Secure Media** in [Media settings](../media/media-settings.md).
- Can't see a menu you expect? See [Roles and permissions](../getting-started/roles-and-permissions.md) and [Troubleshooting](../troubleshooting.md).
