---
title: Manage roles
description: Create roles, choose their permissions feature by feature, read the effective permissions, and delete roles you no longer need.
technical_manual:
  - reference/modules/Roles/README.md
  - reference/modules/Security/Permissions.md
  - topics/security/roles.md
---

# Manage roles

A role is a named set of permissions, such as *Editor* or *Translator*. You give roles to people on their user account, and each person gets every permission of every role they have. Use this page to create roles for the jobs on your site and to choose what each role may do. For the ideas behind roles and the roles that come with a new site, read [Roles and permissions](../getting-started/roles-and-permissions.md) first.

| | |
| --- | --- |
| **Menu** | **Access Control > Roles** |
| **Permission** | *View Roles* to see the list, *Manage Roles* to add, edit and delete roles |
| **Feature** | **Roles** (`OrchardCore.Roles`) |

!!! warning
    *Manage Roles* is marked **Security Critical**: someone who can edit roles can give themselves any permission. Give it only to people you trust with the whole site.

## See the roles

Open **Access Control > Roles**. Each row shows the role name, its description, and a **System** badge for the roles that can't be deleted. Type in **Search** to filter the list by name as you type. With *Manage Roles*, each row has **Edit** and, except for system roles, **Delete**, and the top of the page has **Add Role**.

The grid layout shows the same information in **Name**, **Kind** and **Actions** columns.

## Add a role

1. Open **Access Control > Roles** and click **Add Role**.
2. On **Create Role**, type a **Name**. It is required, must be unique and can't contain `/`.
3. Optionally type a **Description**, shown under the name in the list.
4. Click **Save & Configure** to go straight to the permissions, or **Create** to go back to the list.

The site confirms *Role created successfully.* The name can't be changed later; to rename a role, create a new one and move the users.

!!! tip
    If you create a role with the name of one of the starter roles, such as *Editor* or *Author*, it gets the default permissions that the site's features define for that name.

## Choose a role's permissions

1. Open **Access Control > Roles** and click **Edit** on the role (or click its name).
2. Optionally change the **Description**.
3. Tick the permissions the role needs in the **Allow** column. Type in **Search permissions** to find one quickly.
4. Click **Save** to go back to the list, or open the arrow next to **Save** and choose **and continue** to stay on the page.

The site confirms *Role updated successfully.* To apply the change, the site ends the sessions of everyone who has the role: the next time the site checks their session, they are signed out and sign in again with the new permissions.

### How the page is organized

Permissions are grouped by the feature that provides them. Each group shows the feature name with its id in brackets, for example **Contents (OrchardCore.Contents)**. Content types that allow their own permissions get their own group, such as **Article Content Type - Article**. Only enabled features appear: turn a feature on first to give its permissions.

Each permission is shown by its description, for example *Publish or unpublish content for others*. A red **Security Critical** badge marks permissions that could let someone raise their own access, such as *Manage Roles*, *Assign any role to users* or *Manage security settings and all users*.

!!! note
    A few groups are named after a related feature rather than the one you expect. For example, *Manage Google Authentication settings* is listed under **Google Tag Manager**, *Manage Microsoft Authentication settings* under **Microsoft Entra ID (Azure Active Directory) Authentication**, and *Manage Sign in with X (Twitter) settings* under **X (Twitter) Integration**. Use **Search permissions** when you can't find one.

### Allow and Effective

| Column | Meaning |
| --- | --- |
| **Allow** | The permission is ticked for this role. |
| **Effective** | The role really has the permission: either it is ticked, or a broader ticked permission includes it, or every signed-in user or visitor already has it. |

Point at an **Effective** box to see why: *This permission is granted to this role.*, *This permission is implied by: ...* followed by the broader permission, or *This permission is granted by role: Anonymous, Authenticated*. You can't tick the **Effective** column yourself.

For example, ticking *Edit content for others* also makes *Edit own content* effective, and ticking *Manage security settings and all users* makes *Edit any user*, *Delete any user*, *List all users* and *View user profiles* effective.

### The Administrator role

Editing **Administrator** shows *This system role provides comprehensive access by granting all available permissions.* instead of the list. The role always has every permission, including the ones of features you turn on later. Only its description can be changed.

## Delete a role

1. Open **Access Control > Roles**.
2. Click **Delete** on the role's row and confirm.

The site confirms *Role deleted successfully.* People who had the role lose its permissions and are signed out, the same way as after an edit. **Administrator**, **Authenticated** and **Anonymous** are system roles: they show the **System** badge, have no **Delete** button, and the site refuses to delete them.

## How permissions combine

- A person gets the **union** of the permissions of all their roles. There are no "deny" permissions: a role can only add access.
- Everyone signed in also gets the permissions of **Authenticated**, and everyone at all gets those of **Anonymous**. Be careful: a permission ticked on **Authenticated** is given to every account, including people who registered themselves.
- Broader permissions include narrower ones. The **Effective** column shows the result.
- Some permissions only make sense together. For example, an editor needs *Access admin panel* to open the admin at all, besides the content permissions.
- Permissions "for others" and "own" differ: *Edit own content* covers content items the person created, *Edit content for others* covers everyone's.
- In the **Users** group, permissions such as *Edit users in Editor role* let someone manage only the users in one role. See [Users](users.md#let-someone-manage-only-some-users).

## Default permissions

When a feature is turned on for the first time, it adds its default permissions to the roles that carry the expected names: Administrator, Editor, Author, Contributor, Moderator, Authenticated and Anonymous, when those roles exist. Turning the feature off and on again doesn't overwrite your later changes. Your own roles start with no permissions.

## Other places roles are used

- **Settings > Security > User Login**, on the **Two-Factor Authentication** tab, can require two-factor authentication for chosen roles. See [Login and registration](login-and-registration.md).
- OpenID Connect applications can be given roles for the client credentials flow. See [OpenID Connect server](openid-server.md).
- Workflows can assign or remove a role, or find the users in a role. See the [User activities](../forms-and-automation/workflow-activities.md#user).
- Deployment plans can export and import all roles. See [Import and export](../administration/import-and-export.md).

If the role editor shows a red **Duplicate Permissions Detected** box, two modules define a permission with the same name. That is a problem for your developer to fix.

!!! example "See it in practice"
    [Give an editor limited access](../use-cases/editor-with-limited-access.md) and [Set up content approval](../use-cases/content-approval.md) walk through this step by step.
