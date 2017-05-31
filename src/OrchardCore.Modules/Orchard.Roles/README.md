# CONFIGURATION
Roles can be configured through the roles menu in admin dasboard but also through a recipe step.

A sample of roles configuration step:
```
{
    "name": "roles",
    "Roles": [
        {
            "Name": "Journalist",
            "Permissions": [ "PublishContent", "EditContent" ]
        },
        {
            "Name": "Suscriptor",
            "Permissions": [ ]
        },
    ]
}
```