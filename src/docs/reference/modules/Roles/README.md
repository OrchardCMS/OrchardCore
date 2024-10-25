# Roles (`OrchardCore.Roles`)

Enabling the `OrchardCore.Roles` module will allow you to manage the user roles.

## Predefined Roles

Orchard Core defines the following predefined permission stereotypes:

| Name | Description |
| --- | --- |
| `Administrator` | A **system role** that grants all permissions to the assigned users. |
| `Anonymous` | A **system role** representing all non-authenticated users. |
| `Authenticated` | A **system role** representing all authenticated users. |
| `Author` | Grants users the ability to create content. |
| `Contributor` | Grants users the ability to contribute content. |
| `Editor` | Grants users the ability to edit existing content. |
| `Moderator` | Grants users the ability to moderate content. |

!!! note
    System roles cannot be deleted, and the `Administrator` role cannot be edited.

## Roles Configuration

Roles can be created and configured through the roles menu in the admin dashboard, but also through a recipe step. Note that roles for features are not predefined. They are created out of predefined permission stereotypes.

A sample of a roles configuration step:

```json
{
    "name": "roles",
    "Roles": [
        {
            "Name": "Journalist",
            "Description" "Journalist Role",
            "Permissions": ["PublishContent", "EditContent"]
        },
        {
            "Name": "Subscriber",
            "Description" "Subscriber Role",
            "Permissions": []
        }
    ]
}
```

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/FmgZHpFHCcg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/2O1F7pwUrTY" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/PY61oZm6mBo" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
