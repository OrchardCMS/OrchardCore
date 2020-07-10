# Markdown (`OrchardCore.Markdown`)

## Theming

### Shapes

The following shapes are rendered when the `MarkdownBodyPart` is attached to a content type:

| Name | Display Type | Default Location | Model Type |
| ------| ------------ |----------------- | ---------- |
| `MarkdownBodyPart` | `Detail` | `Content:5` | `MarkdownBodyPartViewModel` |
| `MarkdownBodyPart` | `Summary` | `Content:10` | `MarkdownBodyPartViewModel` |

### `BodyPartViewModel`

The following properties are available on the `MarkdownBodyPartViewModel` class.

| Property | Type | Description |
| --------- | ---- |------------ |
| `Markdown` | `string` | The Markdown value after all tokens have been processed. |
| `Html` | `string` | The HTML content resulting from the Markdown source. |
| `ContentItem` | `ContentItem` | The content item of the part. |
| `MarkdownBodyPart` | `MarkdownBodyPart` | The `MarkdownBodyPart` instance. |
| `TypePartSettings` | `MarkdownBodyPartSettings` | The settings of the part. |

### `MarkdownBodyPart`

The following properties are available on `MarkdownBodyPart`:

| Name | Type | Description |
| -----| ---- |------------ |
| `Markdown` | The Markdown content. It can contain Liquid tags so using it directly might result in unexpected results. Prefer rendering the `MarkdownBodyPart` shape instead. |
| `Content` | The raw content of the part. |
| `ContentItem` | The content item containing this part. |

### `MarkdownField`

This shape is rendered when a `MarkdownField` is attached to a content part.
The shape base class is of type `MarkdownFieldViewModel`.

The following properties are available on the `MarkdownFieldViewModel` class.

| Property | Type | Description |
| --------- | ---- |------------ |
| `Markdown` | `string` | The Markdown value once all tokens have been processed. |
| `Html` | `string` | The HTML content resulting from the Markdown source. |
| `Field` | `MarkdownField` | The `MarkdownField` instance. |
| `Part` | `ContentPart` | The part this field is attached to. |
| `PartFieldDefinition` | `ContentPartFieldDefinition` | The part field definition. |

## Editors

The __Markdown Part__ editor can be different for each content type. In the __Markdown Part__ settings of a 
content type, just select the one that needs to be used.

There are two predefined editor names:

- `Default` is the editor that is used by default.
- `Wysiwyg` is the editor that provides a WYSIWYG experience.

### Custom Editors

Customizing the editor can mean to replace the predefined one with different experiences, or to provide
new options for the user to choose from.

To create a new custom editor, it is required to provide two shape templates, one to provide
the name of the editor (optional if you want to override an existing one), and a shape to
render the actual HTML for the editor.

#### Declaration

To declare a new editor, create a shape named `Markdown_Option__{Name}` where `{Name}` is a value 
of your choosing. This will be represented by a file named `Markdown-{Name}.Option.cshtml`.

Sample content:

```csharp
@{
    string currentEditor = Model.Editor;
}
<option value="Wysiwyg" selected="@(currentEditor == "Wysiwyg")">@T["Wysiwyg editor"]</option>
```

#### HTML Editor

To define what HTML to render when the editor is selected from the settings, a shape named 
`Markdown_Edit__{Name}` corresponding to a file `Markdown-{Name}.Edit.cshtml` can be created.

Sample content:

```csharp
@using OrchardCore.Markdown.ViewModels;
@model MarkdownBodyPartViewModel

<fieldset class="form-group">
    <label asp-for="Markdown">@T["Markdown"]</label>
    <textarea asp-for="Markdown" rows="5" class="form-control"></textarea>
    <span class="hint">@T["The markdown of the content item."]</span>
</fieldset>
```

### Overriding the predefined editors

You can override the HTML editor for the `Default` editor by creating a shape file named 
`Markdown.Edit.cshtml`. The WYSIWYG editor is defined by using the file named 
`Markdown-Wysiwyg.Edit.cshtml`.

## Razor Helper

To render a Markdown string to HTML within Razor use the `MarkdownToHtmlAsync` helper extension method on the view's base `Orchard` property, e.g.:

```csharp
@await Orchard.MarkdownToHtmlAsync((string)Model.ContentItem.Content.MarkdownParagraph.Content.Markdown)
```

In this example we assume that `Model.ContentItem.Content.MarkdownParagraph.Content` represents an `MarkdownField`, and `Markdown` is the field value, and we cast to a string, as extension methods do not support dynamic dispatching.

This helper will also parse any liquid included in the Markdown.

## CREDITS

### Markdig

<https://github.com/lunet-io/markdig>  
Copyright (c) 2016, Alexandre Mutel  
BSD-2

### SimpleMDE

<https://github.com/sparksuite/simplemde-markdown-editor>  
Copyright (c) 2015 Next Step Webs, Inc.  
MIT
