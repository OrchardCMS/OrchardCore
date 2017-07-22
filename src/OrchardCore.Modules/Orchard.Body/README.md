# Body (Orchard.Body)

## Editors

The __Body Part__ editor can be different for each content type. In the __Body Part__ settings of a 
content type, just select the one that needs to be used.

There are two predefined editor names:
- `Default` is the editor that is used by default
- `Wysiwyg` is the editor that provides a WYSIWYG experience

### Custom Editors

Customizing the editor can mean to replace the predefined one with different experiences, or to provide
new options for the user to choose from.

To create a new custom editor, it is required to provide two shape templates, one to provide
the name of the editor (optional if you want to override and existing one), and a shape to
render the actual HTML for the editor.

#### Declaration

To declare a new editor, create a shape named `Body_Option__{Name}` where `{Name}` is a value 
of your choosing. This will be represented by a file named `Body-{Name}.Option.cshtml`.

Sample content:

```csharp
@{
    string currentEditor = Model.Editor;
}
<option value="Wysiwyg" selected="@(currentEditor == "Wysiwyg")">@T["Wysiwyg editor"]</option>
```

#### HTML Editor

To define what HTML to render when the editor is selected from the settings, a shape named 
`Body_Editor__{Name}` corresponding to a file `Body-{Name}.Editor.cshtml` can be created.

Sample content:

```csharp
@using Orchard.Body.ViewModels;
@model BodyPartViewModel

<fieldset class="form-group">
    <label asp-for="Body">@T["Body"]</label>
    <textarea asp-for="Body" rows="5" class="form-control"></textarea>
    <span class="hint">@T["The body of the content item."]</span>
</fieldset>
```

### Overriding the predefined editors

You can override the HTML editor for the `Default` editor by creating a shape file named 
`Body.Editor.cshtml`. The Wysiwyg editor is defined by using the file named 
`Body-Wysiwyg.Editor.cshtml`.

## CREDITS

### Trumbowyg
https://github.com/Alex-D/Trumbowyg
Copyright (c) 2012-2016 Alexandre Demode (Alex-D)
License: MIT
