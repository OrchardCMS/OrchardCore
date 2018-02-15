# Content Fields (OrchardCore.ContentFields)

## Purpose

This modules provides common content fields.

## Available Fields

| Name | Properties | Shape Type |
| --- | --- | --- |
| `TextField` | `Text (string)` | `TextField` |
| `BooleanField` | `Value (bool)` | `BooleanField` |
| `HtmlField` | `Html (string)` | `HtmlField` |
| `NumericField` | `Value (decimal?)` | `NumericField` |

## Usage

From a `Content` template, you can reference a field's value like this
(if the content type is `Article` and has a Text Field named `MyField`):

```csharp
var fieldValue = contentItem.Content.Article.MyField.Text;
```

## Creating Custom Fields

#### What to extend?
Before creating a new field the solution might be to provide a custom editor and formatter 
instead.

A field should represent some specific physical data and logical data. The same field can be customized
to be edited and rendered differently using both Editors and Formatters. Editors are shapes that can
be used to edit a field differently, for instance the WYSIWYG HTML editor is a custom editor for the HTML
field. Formatters are alternate shapes that can be used to render a field on the front end, for instance
a Link field could be rendered as a Youtube video player. 

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
services.AddSingleton<ContentField, TextField>();
```

### Display Driver

The display driver is the component that drives how the field is displayed on the front end, edited on
the admin, updated and validated.

Create a class inheriting from `ContentFieldDisplayDriver<TextField>` and implement the three methods 
`Display`, `Edit` and `DisplayAsync` by looking at examples from this module.

This class needs to be registered in the DI like this:
```csharp
services.AddScoped<IContentFieldDisplayDriver, TextFieldDisplayDriver>();
```

## Creating Custom Editors

For each field, the convention is to create an alternate that can target different editors. To provide
a new choice in the list of available editors for a field, create a new shape template that matches this
template: `{FIELDTYPE}_Option__{EDITORNAME}`
This shape type will match a template file named `{FIELDTYPE}-{EDITORNAME}.Option.cshtml`

This template will need to render an `<option>` tag. Here is an example for a Wysiwyg options on the 
Html Field:
```csharp
@{
    string currentEditor = Model.Editor;
}
<option value="Wysiwyg" selected="@(currentEditor == "Wysiwyg")">@T["Wysiwyg editor"]</option>
```

Then you can create the editor shape by adding a file named `{FIELDTYPE}_Editor__{EDITORNAME}` which is
represented by a template file named `{FIELDTYPE}-{EDITORNAME}.Editor.cshtml`. 

For instance the filename for the Wysiwyg editor on the Html Field is named `HtmlField-Wysiwyg.Editor.cshtml`.

## CREDITS

### bootstrap-slider
<https://github.com/seiyria/bootstrap-slider>  
Copyright (c) 2017 Kyle Kemp, Rohit Kalkur, and contributors  
License: MIT

### Bootstrap Switch
<https://github.com/Bttstrp/bootstrap-switch>  
Copyright (c) 2013-2015 The authors of Bootstrap Switch  
License: MIT
