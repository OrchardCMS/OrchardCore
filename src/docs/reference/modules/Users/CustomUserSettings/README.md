# Custom Users Settings (`OrchardCore.User.CustomUserSettings`)

Custom Users Settings allow a site administrator to create a customized set of properties for a given user.  

## Creating Custom User Settings

Custom User Settings are organized in sections, each section is represented by a Content Type with the `CustomUserSettings` stereotype. 

From the "Features" list, activate the module `Custom User Settings`. To add a new section is necessary to create a new Content Type (called e.g. "User Settings"), disable `Creatable`, `Listable`, `Draftable` and `Securable` metadata as they don't apply, and add a field, or parts, that we want as a new property for our users (e.g. we can add a "Text Field" to show user nicknames). 

!!! warning
    Don't mark any existing Content Type with this `CustomUserSettings` stereotype, as this will break existing content items of this type.

Custom User Settings are then comprised of parts and fields like any other content type.  
Once created, open the user list, create or edit an existing user, and each of these sections should appear alongside the module-provided ones. For more details see the video at the [bottom of this page](#Video). 

## Usage

### Liquid

The Custom User Settings are available when loading the user from the database.

```liquid
{% assign user = User | user_id | users_by_id %}
{{user.Properties}}
```

Each section is made available using its name.

For instance for a custom settings section named `UserProfile`, with a `TextField` named `FirstName` would be accessible using `{{ user.Properties.UserProfile.UserProfile.FirstName.Text }}`.

### Placement

By default each Custom User Settings Content Type is placed in a tab.

To adjust the placement, for example, to move the setting out of the tab, use the `Differentiator` of `CustomUserSettings-PartDefinitionName`.

``` json
{
  "CustomUserSettings": [
    {
      "place": "Content:10#Content",
      "differentiator": "CustomUserSettings-UserProfile" 
    }
  ]
}
```

## User Section Display Driver

You may also extend the `User` properties by implementing a `SectionDisplayDriver<User, UserProfile>` where `User` is the type of entity to be edited,
and `UserProfile` is the property to extend the `User` entity with.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/_ff79hm5PAc" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
