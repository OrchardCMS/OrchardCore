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

Access field values in templates:

=== "Liquid"
    ```liquid
    {{ Model.ContentItem.Content.Article.MyField.Text }}
    ```

=== "Razor"
    ```html
    var fieldValue = Model.ContentItem.Content.Article.MyField.Text;
    ```

From a field's shape (see Shape Type in the table listing all fields), you can also access properties specific to each view model.

### Common Field Properties

All field view models expose these properties:

| Property              | Description                                                                                          |
|-----------------------|------------------------------------------------------------------------------------------------------|
| `Field`               | The ContentField instance                                                                           |
| `Part`                | The ContentPart containing the field                                                                |
| `PartFieldDefinition` | The field definition (includes access to Content Type)                                              |

Some view models have special properties that are computed from the actual field data and which are more useful for templating.

## Field Examples

### `HtmlField`

#### `DisplayHtmlFieldViewModel`

| Property | Description                                                   |
|----------|---------------------------------------------------------------|
| `Html`   | Processed HTML (after liquid tag processing)                  |

#### Example
```liquid
{{ Model.Html }}  {# Processed HTML #}
{{ Model.Field.Html }}  {# Raw HTML #}
```

### `DateTimeField`

#### `DisplayDateTimeFieldViewModel`
| Property        | Description                                 |
|-----------------|---------------------------------------------|
| `LocalDateTime` | DateTime in site's time zone                |

#### Example
```liquid
{{ Model.LocalDateTime }}  {# Local time #}
{{ Model.Value }}  {# UTC time #}
```

### `ContentPickerField`

#### Example
=== "Liquid"
    ```liquid
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

### `LinkField`

#### `LinkFieldViewModel`
| Property | Description                             |
|----------|-----------------------------------------|
| Url      | Valid URI                               |
| Text     | Display text                            |
| Target   | Anchor target attribute                 |

!!! note
    Only http/https URIs allowed by default. Configure [HTML Sanitizer](../Sanitizer/README.md) for mailto/tel.

#### Example
=== "Liquid"
    ```liquid
    <a href='{{Model.Url}}' Target={{Model.Target}}>{{Model.Text}}</a>
    ```

=== "Razor"
    ```html
    <a href="@Model.Url" target="@Model.Target">@Model.Text</a>
    ```

### `LocalizationSetContentPickerField`

For culture-neutral content references:

=== "Liquid"
    ```liquid
    {% assign contentItems = Model.LocalizationSets | localization_set %}
    {% for contentItem in contentItems %}
        {{ contentItem.DisplayText }}
    {% endfor %}
    ```

=== "Razor"
    ```html
    @{
        var currentCulture = Context.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
        var contentItems = await ContentLocalizationManager.GetItemsForSetsAsync(Model.LocalizationSets, currentCulture);
    }
    @foreach (var contentItem in contentItems)
    {
        <span class="value">@contentItem.DisplayText</span>
        @if (contentItem != contentItems.Last())
        {
            <span>,</span>
        }
    }
    ```

### `UserPickerField`

Configure to show all users or filter by roles.

#### Example
=== "Liquid"
    ```liquid
    {% assign users = Model.UserIds | users_by_id %}
    {% for user in users %}
        {{ user.UserName }} - {{ user.Email }}
    {% endfor %}
    ```

=== "Razor"
    ```html
    @{
        var users = await @Orchard.GetUsersByIdsAsync(Model.UserIds);
    }
    @foreach (var user in users)
    {
        <span class="value">@user.UserName</span>
        @if (user != users.Last())
        {
            <span>,</span>
        }
    }
    ```

#### Video
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/vqXwK69vtMw" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

### `MultiTextField`
#### Video
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/WfP_rXz1id0" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Creating Custom Fields

### When to Extend

Consider creating custom editors/formatters before new fields. Fields should represent specific data types, while editors/formatters handle input/output variations.

### Model Class

Create a class inheriting from `ContentField`:

```csharp
public class TextField : ContentField
{
    public string Text { get; set; }
}
```

Register in DI:
```csharp
services.AddContentField<TextField>();
```

### Display Driver

Create a driver inheriting from `ContentFieldDisplayDriver<TField>`:

```csharp
public class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
{
    // Implement Display, Edit, and UpdateAsync methods
}
```

Register in DI:
```csharp
services.AddContentField<TextField>()
    .UseDisplayDriver<TextFieldDisplayDriver>();
```

## Creating Custom Display Modes

1. Create display option template (`{FieldType}-{DisplayMode}.DisplayOption.cshtml`):
```html
@{
    string currentDisplayMode = Model.DisplayMode;
}
<option value="Header" selected="@(currentDisplayMode == "Header")">@T["Header"]</option>
```

2. Create display shape template (`{FieldType}-{DisplayMode}.Display.cshtml`)

## Creating Custom Editors

1. Create editor option template (`{FieldType}-{EditorName}.Option.cshtml`):
```html
@{
    string currentEditor = Model.Editor;
}
<option value="Wysiwyg" selected="@(currentEditor == "Wysiwyg")">@T["Wysiwyg editor"]</option>
```

2. Create editor shape template (`{FieldType}-{EditorName}.Edit.cshtml`)

### Custom Driver Registration

Override drivers for specific modes:

```csharp
// For display modes
services.AddContentField<TextField>()
    .ForDisplayMode<TextFieldDisplayDriver>(d => String.IsNullOrEmpty(d))
    .ForDisplayMode<CustomDriver>(d => d == "CustomMode");

// For editors
services.AddContentField<TextField>()
    .ForEditor<TextFieldDisplayDriver>(d => d != "CustomEditor")
    .ForEditor<CustomDriver>(d => d == "CustomEditor");
```

!!! note
    Add module dependency in `Manifest.cs` to ensure your registrations override defaults.

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
