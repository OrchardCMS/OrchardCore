# Roles (`OrchardCore.Roles`)

Enabling the `OrchardCore.Roles` module will allow you to manage the user roles.

## Predefined Roles

Orchard Core defines the following predefined permission stereotypes:

| Name            | Description                                                          |
|-----------------|----------------------------------------------------------------------|
| `Administrator` | A **system role** that grants all permissions to the assigned users. |
| `Anonymous`     | A **system role** representing all non-authenticated users.          |
| `Authenticated` | A **system role** representing all authenticated users.              |
| `Author`        | Grants users the ability to create content.                          |
| `Contributor`   | Grants users the ability to contribute content.                      |
| `Editor`        | Grants users the ability to edit existing content.                   |
| `Moderator`     | Grants users the ability to moderate content.                        |

!!! note
    System roles cannot be deleted, and the `Administrator` role cannot be edited.

## Roles Configuration

Roles can be created and configured through the roles menu in the admin dashboard, but also through a recipe step. Note that roles for features are not predefined. They are created out of predefined permission stereotypes.

A sample of a roles configuration step:

```json
{
    "steps": [
        {
            "name": "roles",
            "Roles": [
                {
                    "Name": "Journalist",
                    "Description": "Journalist Role",
                    "PermissionBehavior": "Replace",
                    "Permissions": ["PublishContent", "EditContent"]
                },
                {
                    "Name": "Subscriber",
                    "Description": "Subscriber Role",
                    "PermissionBehavior": "Replace",
                    "Permissions": []
                }
            ]
        }
    ]
}
```

The `Roles` recipe step supports specific permission behaviors so you can control how permissions are managed within a role. The following behaviors are available:

- **Replace**: This behavior removes all existing permissions associated with the role and replaces them with the new permissions from the `Permissions` collection. This is the default behavior.
- **Add**: This behavior adds the new permission(s) from the `Permissions` collection to the role, but only if they do not already exist. Existing permissions are left unchanged.
- **Remove**: This behavior removes the specified permission(s) from the role's existing permissions based on the `Permissions` collection.

### Example: Adding a New Permission to a Role

For instance, to add the "CanChat" permission to the `Subscriber` role, use the following configuration:

```json
{
    "steps": [
        {
            "name": "roles",
            "Roles": [
                {
                    "Name": "Subscriber",
                    "PermissionBehavior": "Add",
                    "Permissions": ["CanChat"]
                }
            ]
        }
    ]
}
```

## Declaring permissions

A module declares its permissions in an `IPermissionProvider`. Create the description with `LocalizedString.Create()`, and pass the type that declares the permission as the translation context:

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace MyModule;

public static class MyPermissions
{
    public static readonly Permission ManageWidgets = new(
        "ManageWidgets",
        LocalizedString.Create("Manage widgets", typeof(MyPermissions)));
}
```

The roles editor translates the description with the full name of that type as the context, for example `MyModule.MyPermissions`, so a PO file entry for the description uses `msgctxt "MyModule.MyPermissions"`. When there is no PO file translation, the roles editor uses the Data Localization translation of the description, if there is one.

A description that is a plain `string` is still supported, for example `new Permission("ManageWidgets", "Manage widgets")`. It is not translated with PO files, but it can still be translated with Data Localization. Descriptions that are templates, for example `"Edit {0}"`, stay plain strings, because the format arguments are applied before the description is shown.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/FmgZHpFHCcg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/2O1F7pwUrTY" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/PY61oZm6mBo" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
