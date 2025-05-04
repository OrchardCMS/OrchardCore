# Content Fields (`OrchardCore.ContentFields`)

This module provides common content fields for capturing data in Orchard Core. Fields are typically added to a content part, which is then added to a content type. 

!!! note
    While the UI allows adding fields directly to content types, behind the scenes it creates a content part with the same name as the content type and attaches the fields to it.

Fields support multiple editors - different UIs for data input. For example, a TextField can have editors like `Email`, `TextArea`, or `Tel`. Editors can be configured in the field settings.

## Available Fields

| Name                                | Properties                                                     | Available Editors |
|-------------------------------------|----------------------------------------------------------------|-------------------|
| `BooleanField`                      | `bool Value`                                                   | (Standard ``), `Switch`|
| `ContentPickerField`                | `string[] ContentItemIds`                                      | (Standard ``)           |
| `DateField`                         | `DateTime? Value`                                              | (Standard ``), `Localized`        |
| `DateTimeField`                     | `DateTime? Value`                                              | (Standard ``)    |
| `HtmlField`                         | `string Html`                                                  | (Standard ``), `Monaco`, `Multiline`, `Trumbowyg`, `Wysiwyg` |
| `LinkField`                         | `string Url, string Text`                                      | (Standard ``)           |
| `LocalizationSetContentPickerField` | `string[] LocalizationSets`                                    | (Standard ``)           |
| `MarkdownField`                     | `string Markdown`                                              | (Standard ``), `Wysiwyg`     |
| `MediaField`                        | `string[] Paths`                                               | (Standard ``), `Attached`       |
| `MultiTextField`                    | `string[] Values`                                              | (Standard ``), `CheckboxList`, `Picker`           |
| `NumericField`                      | `decimal? Value`                                               | (Standard ``), `Range`, `Select`, `Slider`, `Spinner`       |
| `GeoPointField`                     | `decimal Latitude, decimal Longitude`                          | (Standard ``), `Leaflet`         |
| `TaxonomyField`                     | `string TaxonomyContentItemId, string[] TaxonomyContentItemId` | (Standard ``), `Tags`         |
| `TextField`                         | `string Text`                                                  | (Standard ``), `CodeMirror`, `Color`, `Email`, `Header`, `IconPicker`, `Monaco`, `PredefinedList`, `Tel` , `TextArea` , `Url` |
| `TimeField`                         | `TimeSpan? Value`                                              | (Standard ``)        |
| `UserPickerField`                   | `string[] UserIds`                                             | (Standard ``), `UserNames`      |
| `YoutubeField`                      | `string EmbeddedAddress, string RawAddress`                    | (Standard ``)          |

!!! note
    Each field is rendered by a corresponding `Shape Type` that uses its own Display view model.  
    Example: `BooleanField` uses shape type `BooleanField` with `DisplayBooleanFieldViewModel`.

## Editors and Display Modes

Fields support different editors for input and display modes for output:

- **Editors**: Control how data is entered (e.g., WYSIWYG for HTML, DatePicker for dates)
- **Display Modes**: Control how data is rendered (e.g., different formatting for dates)

You can create custom editors and display modes by following the patterns in the "Creating Custom Fields" section.

## Usage

From a `Content` template, you can reference a field's value like this
(if the content type is `Article` and has a Text Field named `MyField`):

=== "Liquid"

    ``` liquid
    {{ Model.ContentItem.Content.Article.MyField.Text }}
    ```

=== "Razor"

    ``` html
    var fieldValue = Model.ContentItem.Content.Article.MyField.Text;
    ```

From a field shape (see Shape Type in the table listing all the fields) you can also access properties specific to each view model.

### Common field properties

The convention for a field view model is to also expose these properties:

| Property              | Description                                                                                          |
|-----------------------|------------------------------------------------------------------------------------------------------|
| `Field`               | The ContentField.                                                                                    |
| `Part`                | The ContentPart that contains the field.                                                             |
| `PartFieldDefinition` | The Content Part Field Definition that contains the part. Which also give access to the Content Type |

Some view models have special properties that are computed from the actual field data and which are more useful for templating.

### `HtmlField`

#### `DisplayHtmlFieldViewModel`

| Property | Description                                                   |
|----------|---------------------------------------------------------------|
| `Html`   | The processed HTML, once all liquid tags have been processed. |

#### Html Field Example

``` liquid
{{ Model.Html }}
```

or, to display the raw content before the tags are converted:

``` liquid
{{ Model.Field.Html }}
```

### `DateTimeField`

#### `DisplayDateTimeFieldViewModel`

| Property        | Description                                 |
|-----------------|---------------------------------------------|
| `LocalDateTime` | The date time in the time zone of the site. |

#### DateTime Field Example

``` liquid
{{ Model.LocalDateTime }}
```

or, to display the UTC value before it is converted:

``` liquid
{{ Model.Value }}
```

### `ContentPickerField`

#### ContentPicker Field Example

=== "Liquid"

    ``` liquid
    {% assign contentItems = Model.ContentItemIds | content_item_id %}
    {% for contentItem in contentItems %}
        {{ contentItem.DisplayText }}
    {% endfor %}
    ```

=== "Razor"

    ```html
    @foreach (var contentItem in await Orchard.GetContentItemsByIdAsync(Model.ContentItemIds))
    {
        @contentItem.DisplayText
    }
    ```

Or to render the referenced content item:

=== "Liquid"

    ``` liquid
    {% assign contentItems = Model.ContentItemIds | content_item_id %}
    {% for contentItem in contentItems %}
        {{ contentItem | shape_build_display: "Detail" | shape_render }}
    {% endfor %}
    ```

=== "Razor"

    ``` html
    @foreach (var contentItem in await Orchard.GetContentItemsByIdAsync(Model.ContentItemIds))
    {
        @await Orchard.DisplayAsync(contentItem, "Detail")
    }
    ```
### `LinkField`

Field used to display a link.

#### `LinkFieldViewModel`

| Property | Description                                                                                                           |
|----------|-----------------------------------------------------------------------------------------------------------------------|
| Url      | A valid URI for the href                                                                                              |
| Text     | The text to display                                                                                                   |
| Target   | The target attribute for the anchor tag, [Target](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/a#target) |

!!! Note: By default the http and https are the only URIs permitted. In order to use other URIs like mailto and tel you must configure the [HTML Sanitizer](../Sanitizer/README.md). 

#### Link Field Example 

=== "Liquid"

``` liquid
<a href='{{Model.Url}}' Target={{Model.Target}}>{{Model.Text}}</a>
```

=== "Razor"

```html
<a href="@Model.Url" target="@Model.Target">@Model.Text</a>
```

### `LocalizationSetContentPickerField`

This field allows you to store the `LocalizationSet` of a `ContentItem`, when a reference shouldn't point to a specific culture of a content item.  
This simplifies getting a content item of the correct culture on the frontend.

The following example uses the `localization_set` liquid filter which returns a single ContentItem
per set based on the request culture, if no culture is specified.

#### LocalizationSet ContentPicker Field Example

=== "Liquid"

    ```liquid
    {% assign contentItems = Model.LocalizationSets | localization_set %}
    {% for contentItem in contentItems %}
        {{ contentItem.DisplayText }}
    {% endfor %}
    ```

=== "Razor"

    ``` html
    @model OrchardCore.ContentFields.ViewModels.DisplayLocalizationSetContentPickerFieldViewModel
    @using Microsoft.AspNetCore.Localization

    @inject OrchardCore.ContentLocalization.IContentLocalizationManager ContentLocalizationManager;

    @{
        var currentCulture = Context.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
        var contentItems = await ContentLocalizationManager.GetItemsForSetsAsync(Model.LocalizationSets, currentCulture);
    }
    foreach (var contentItem in contentItems)
    {
        <span class="value">@contentItem.DisplayText</span>
        if (contentItem != contentItems.Last())
        {
            <span>,</span>
        }
    }
    ```

### `UserPickerField`

The User Picker field allows you to relate users to a content item.

When adding the field to a content type, use the settings to specify whether to

- List all users,
- List users from specific roles.

#### UserPicker Field Example

=== "Liquid"

    ```liquid
    {% assign users = Model.UserIds | users_by_id %}
    {% for user in users %}
        {{ user.UserName }} - {{ user.Email }}
    {% endfor %}
    ```

=== "Razor"

    ``` html
    @model OrchardCore.ContentFields.ViewModels.DisplayUserPickerFieldViewModel
    @using OrchardCore.Mvc.Utilities

    @{
        var name = (Model.PartFieldDefinition.PartDefinition.Name + "-" + Model.PartFieldDefinition.Name).HtmlClassify();
        var users = await @Orchard.GetUsersByIdsAsync(Model.UserIds);
    }

    <div class="field field-type-userpickerfield field-name-@name">
        <span class="name">@Model.PartFieldDefinition.DisplayName()</span>
        @if (users.Any())
        {
            foreach (var user in users)
            {
                <span class="value">@user.UserName</span>
                if (user != users.Last())
                {
                    <span>,</span>
                }
            }
        }
        else
        {
            <span class="value">@T["No users."]</span>
        }
    </div>

    ```

#### Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/vqXwK69vtMw" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

### `MultiText Field`

#### Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/WfP_rXz1id0" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Creating Custom Fields

### What to extend

Before creating a new field, the solution might be to provide a custom editor and formatter instead.

A field should represent some specific physical data and logical data. The same field can be customized
to be edited and rendered differently using both Editors and Formatters. Editors are shapes that can
be used to edit a field differently, for instance the WYSIWYG HTML editor is a custom editor for the HTML
field. Formatters are alternate shapes that can be used to render a field on the front end, for instance
a `Link` field could be rendered as a Youtube video player.

### Model Class

Create a class inheriting from `ContentField` that will represent the state of your field.
Its content will be serialized as part of the content item.
Json.NET classes can be used to customize the serialization.

Example:

```csharp
public class TextField : ContentField
{
    public string Text { get; set; }
}
```

This class needs to be registered in the DI like this:

```csharp
services.AddContentField<TextField>();
```

### Display Driver

The display driver is the component that drives how the field is displayed on the front end, edited on
the admin, updated and validated.

Create a class inheriting from `ContentFieldDisplayDriver<TextField>` and implement the three methods
`Display`, `Edit` and `DisplayAsync` by looking at examples from this module.

This class needs to be registered in the DI like this:

```csharp
services.AddContentField<TextField>()
    .UseDisplayDriver<TextFieldDisplayDriver>();
```

This will register the display driver for use with all display modes and editors.

## Creating Custom Display Modes

For each field, the convention is to create an alternate that can target different display modes. To provide
a new choice in the list of available editors for a field, create a new shape template that matches this
TextField-Header.DisplayOption
template: `{FIELDTYPE}_DisplayOption__{DISPLAYMODE}`
This shape type will match a template file named `{FIELDTYPE}-{DISPLAYMODE}.DisplayOption.cshtml`

This template will need to render an `<option>` tag. Here is an example for a Header display mode options on the Text Field:

``` html
@{
    string currentDisplayMode = Model.DisplayMode;
}
<option value="Header" selected="@(currentDisplayMode == "Header")">@T["Header"]</option>
```

Then, you can create the display mode shape by adding a file named `{FIELDTYPE}_Display__{DISPLAYMODE}` which is
represented by a template file named `{FIELDTYPE}-{DISPLAYMODE}.Display.cshtml`.

For instance, the filename for the Header Display Mode on the Text Field is named `TextField-Header.Display.cshtml`.

## Creating Custom Editors

For each field, the convention is to create an alternate that can target different editors. To provide
a new choice in the list of available editors for a field, create a new shape template that matches this
template: `{FIELDTYPE}_Option__{EDITORNAME}`
This shape type will match a template file named `{FIELDTYPE}-{EDITORNAME}.Option.cshtml`

This template will need to render an `<option>` tag. Here is an example for a Wysiwyg options on the Html Field:

``` html
@{
    string currentEditor = Model.Editor;
}
<option value="Wysiwyg" selected="@(currentEditor == "Wysiwyg")">@T["Wysiwyg editor"]</option>
```

Then you can create the editor shape by adding a file named `{FIELDTYPE}_Edit__{EDITORNAME}` which is
represented by a template file named `{FIELDTYPE}-{EDITORNAME}.Edit.cshtml`.

For instance the filename for the Wysiwyg editor on the Html Field is named `HtmlField-Wysiwyg.Edit.cshtml`.

### Customising Display Driver Registration

For both Display Modes and Editors you can customise the Display Driver that will be resolved for the particular mode.

This allows you to create custom display drivers that might return a different ViewModel to the standard Display Driver.

Alter the registration for an existing Field Type or Driver Type in your modules `Startup.cs`

```csharp
services.AddContentField<TextField>()
    .ForDisplayMode<TextFieldDisplayDriver>(d => String.IsNullOrEmpty(d))
    .ForDisplayMode<MyCustomTextFieldDisplayDriver>(d => d == "MyCustomDisplayMode");
```

This example will alter the registration for the `TextFieldDisplayDriver` to resolve for only the Standard (null)
display mode, and register `MyCustomTextFieldDisplayDriver` to resolve for only the custom display mode.

```csharp
services.AddContentField<TextField>()
    .ForEditor<TextFieldDisplayDriver>(d => d != "MyCustomEditor")
    .ForEditor<MyCustomTextFieldDisplayDriver>(d => d == "MyCustomEditor");
```

This example will alter the registration for the `TextFieldDisplayDriver` to resolve for all editors except the custom editor,
and register `MyCustomTextFieldDisplayDriver` to resolve for only the custom editor.

!!! note
    When registering a custom display mode or editor driver you must alter the registrations for existing drivers.
    You should also take a dependency in your modules `Manifest.cs` on the module that the fields reside in.
    This will make your modules `Startup.cs` run later, and allow your registrations to override the original modules.

## Recipes

Example recipe snippet for creating fields:

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Person",
          "DisplayName": "Person",
          "Settings": {
            "ContentTypeSettings": {
              "Creatable": true,
              "Listable": true,
              "Draftable": true,
              "Versionable": true,
              "Securable": true
            },
            "FullTextAspectSettings": {
              "IncludeFullTextTemplate": false,
              "IncludeBodyAspect": true,
              "IncludeDisplayText": true
            }
          },
          "ContentTypePartDefinitionRecords": [
            {
              "PartName": "Person",
              "Name": "Person",
              "Settings": {}
            },
            {
              "PartName": "BiographyPart",
              "Name": "BiographyPart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "1"
                }
              }
            },
            {
              "PartName": "FriendsPart",
              "Name": "FriendsPart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "2"
                }
              }
            },
            {
              "PartName": "PersonPart",
              "Name": "PersonPart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "0"
                }
              }
            },
            {
              "PartName": "ArticlesPart",
              "Name": "ArticlesPart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "3"
                }
              }
            },
            {
              "PartName": "FamilyInfo",
              "Name": "FamilyInfo",
              "Settings": {}
            }
          ]
        }
      ],
      "ContentParts": [
        {
          "Name": "PersonPart",
          "Settings": {
            "ContentPartSettings": {
              "Attachable": true,
              "Reusable": false
            }
          },
          "ContentPartFieldDefinitionRecords": [
            {
              "FieldName": "TextField",
              "Name": "FirstName",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "First Name"
                }
              }
            },
            {
              "FieldName": "TextField",
              "Name": "LastName",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Last Name"
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                },
                "TextFieldSettings": {
                  "Required": true
                }
              }
            },
            {
              "FieldName": "DateField",
              "Name": "DateOfBirth",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Date of Birth"
                }
              }
            },
            {
              "FieldName": "TextField",
              "Name": "Phone",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Phone",
                  "Editor": "Tel"
                },
                "TextFieldSettings": {
                  "Required": false
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                }
              }
            }
          ]
        },
        {
          "Name": "BiographyPart",
          "Settings": {
            "ContentPartSettings": {
              "Attachable": true,
              "Reusable": false
            }
          },
          "ContentPartFieldDefinitionRecords": [
            {
              "FieldName": "TextField",
              "Name": "Biography",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Biography",
                  "Editor": "TextArea",
                  "Position": "0"
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                },
                "TextFieldSettings": {
                  "Required": false
                }
              }
            },
            {
              "FieldName": "LinkField",
              "Name": "LinkedInProfile",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "LinkedIn Profile"
                }
              }
            },
            {
              "FieldName": "LinkField",
              "Name": "GithubProfile",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Github Profile"
                }
              }
            },
            {
              "FieldName": "TextField",
              "Name": "Email",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Email",
                  "Editor": "Email"
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                },
                "TextFieldSettings": {
                  "Required": false
                }
              }
            },
            {
              "FieldName": "MultiTextField",
              "Name": "FavoriteSports",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Favorite Sports",
                  "Editor": "CheckboxList"
                },
                "MultiTextFieldSettings": {
                  "Required": false,
                  "Options": [
                    {
                      "name": "Soccer",
                      "value": "soccer",
                      "default": false
                    },
                    {
                      "name": "Basketball",
                      "value": "basketball",
                      "default": false
                    },
                    {
                      "name": "Hockey",
                      "value": "hockey",
                      "default": false
                    }
                  ]
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                }
              }
            },
            {
              "FieldName": "TextField",
              "Name": "FavoriteColor",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Favorite Color",
                  "Editor": "Color"
                },
                "TextFieldSettings": {
                  "Required": false
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                },
                "TextFieldPredefinedListEditorSettings": {
                  "Options": [
                    {
                      "name": "Soccer",
                      "value": "Soccer"
                    },
                    {
                      "name": "Basketball",
                      "value": "Basketball"
                    },
                    {
                      "name": "Hockey",
                      "value": "Hockey"
                    },
                    {
                      "name": "Football",
                      "value": "Football"
                    }
                  ],
                  "Editor": 1
                }
              }
            },
            {
              "FieldName": "TextField",
              "Name": "Website",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Website",
                  "Editor": "Url"
                },
                "TextFieldSettings": {
                  "Required": false
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                }
              }
            }
          ]
        },
        {
          "Name": "FriendsPart",
          "Settings": {
            "ContentPartSettings": {
              "Attachable": true,
              "Reusable": false
            }
          },
          "ContentPartFieldDefinitionRecords": [
            {
              "FieldName": "UserPickerField",
              "Name": "Friends",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Friends"
                }
              }
            }
          ]
        },
        {
          "Name": "ArticlesPart",
          "Settings": {
            "ContentPartSettings": {
              "Attachable": true,
              "Reusable": false
            }
          },
          "ContentPartFieldDefinitionRecords": [
            {
              "FieldName": "ContentPickerField",
              "Name": "FavoriteArticles",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Favorite Articles"
                },
                "ContentPickerFieldSettings": {
                  "Required": true,
                  "Multiple": true,
                  "DisplayAllContentTypes": false,
                  "DisplayedContentTypes": [
                    "Article"
                  ],
                  "DisplayedStereotypes": [],
                  "TitlePattern": "{{ Model.ContentItem | display_text }}"
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                }
              }
            }
          ]
        },
        {
          "Name": "FamilyInfo",
          "Settings": {
            "ContentPartSettings": {
              "Attachable": true,
              "Reusable": false
            }
          },
          "ContentPartFieldDefinitionRecords": [
            {
              "FieldName": "NumericField",
              "Name": "NumberOfDependents",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Number of dependents",
                  "Editor": "Slider"
                },
                "NumericFieldSettings": {
                  "Required": false,
                  "Scale": 0,
                  "Minimum": 0,
                  "Maximum": 10,
                  "DefaultValue": "0"
                },
                "AzureAISearchContentIndexSettings": {
                  "Included": false
                }
              }
            }
          ]
        }
      ]
    }
  ]
}
```

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/NDUjn5_KdEM" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/bayT58i7DVY" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
