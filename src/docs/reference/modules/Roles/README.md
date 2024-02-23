# Roles (`OrchardCore.Roles`)

Enabling the `OrchardCore.Roles` module will allow you to manage the user roles.

## Predefined Roles

Orchard Core come up with the following predefined permission stereotypes:

| Name | Description |
| --- | --- |
| `Administrator` | Contains all the administrator users. |
| `Anonymous` | Contains all the non authenticated users. |
| `Authenticated` | Contains all the authenticated users. |
| `Author` | Contains all the users who have the ability to author contents. |
| `Contributor` | Contains all the users who have the ability to contribute to the contents. |
| `Editor` | Contains all the users who have the ability to edit the contents. |
| `Moderator` | Contains all the users who have the ability to moderate the contents. |

## Roles Configuration

Roles can be created and configured through the roles menu in the admin dashboard, but also through a recipe step. Note that roles for features are not predefined. They are created out of predefined permission stereotypes.

A sample of a roles configuration step:

```json
{
    "name": "roles",
    "Roles": [
        {
            "Name": "Journalist",
            "Permissions": [ "PublishContent", "EditContent" ]
        },
        {
            "Name": "Subscriber",
            "Permissions": [ ]
        },
    ]
}
```
