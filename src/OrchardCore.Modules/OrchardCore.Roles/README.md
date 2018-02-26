# CONFIGURATION
Roles can be configured through the roles menu in admin dashboard but also through a recipe step.

A sample of a roles configuration step:
```
{
    "name": "roles",
    "Roles": [
        {
            "Name": "Journalist",
            "Permissions": [ "PublishContent", "EditContent" ]
        },
        {
            "Name": "Suscriber",
            "Permissions": [ ]
        },
    ]
}
```
