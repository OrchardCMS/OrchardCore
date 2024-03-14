# Custom Users Settings (`OrchardCore.User.CustomUserSettings`)

Custom Users Settings allow a site administrator to create a customized set of properties for a given user.  

## Creating Custom User Settings

Custom User Settings are organized in sections, each section is represented by a Content Type with the `CustomUserSettings` stereotype. 

To enable the "Custom User Settings" module, select it from the "Features" list. To incorporate a new section, it is essential to generate a fresh Content Type, such as "User Settings." In this process, deactivate the Creatable, Listable, Draftable, and Securable metadata since they are not applicable. Additionally, include a field or components to define the new property for users; for instance, one can append a "Text Field" to display user nicknames.

!!! warning
    Don't mark any existing Content Type with this `CustomUserSettings` stereotype, as this will break existing content items of this type.

Custom User Settings are then comprised of parts and fields like any other content type.  
After creation, go to Security >> Users to either create a new user or modify an existing one. Each of these sections will be accessible through distinct tabs. For further information, refer to the video located at the [bottom of this page](#video). 

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
