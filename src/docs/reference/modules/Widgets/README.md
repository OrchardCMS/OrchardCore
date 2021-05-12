# Widgets (`OrchardCore.Widgets`)

Widgets are content items of a specific category (stereotype) that can be rendered in custom locations of a page.  
The Widgets module provides a `Widget` stereotype and some templates to render it.  
Widgets are used by different modules that need to render specialized pieces of content like Layers or Forms.

## Creating custom Widgets

Many types of Widgets can be created and the default recipes do create some custom ones like `Paragraph`, `Blockquote`, and `MediaWidget` that you can 
use as examples.

Widgets are content items, and as such they can be composed of content fields and content parts. For instance the `Paragraph` widget that is included with the 
`TheBlogTheme` recipe is made out of an HTML field with a WYSIWYG editor.

Widgets can then be composed from the Admin UI during the lifetime of the site, from migrations files to include them as part of custom modules, or recipe files
when a site is set up. The only requirement to create a Widget is to mark a content type with the `Widget` stereotype. By doing so, the different services that look
for Widgets will treat this content item accordingly. This is how the `Layers` module or the Page editor can display the list of available Widget types.

## Theming

Because `Widget` is a stereotype, all Widget content items are rendered from a main shape named `Widget`.
This main shape's template has access to these properties:

| Property | Description |
| --------- | ------------ |
| `Model.ContentItem` | The Widget content item. |
| `Model.Content` | A list of inner shapes to display. It's populated by all the fields and parts the widget is composed of. |

It also contains these specific zones, which are not used most of the time and can be ignored when creating custom templates for the website front-end.

| Property | Description |
| --------- | ------------ |
| `Model.Header` | The shapes to render in the widget's header. |
| `Model.Meta` | The shapes to render in the widget's metadata zone. |
| `Model.Footer` | The shapes to render in the widget's footer. |

The shape also contains all the properties common to all shapes like `Classes`, `Id` and `Attributes`.

### Customizing `Widget` templates

The `Widget` shape is used to render a Widget. The default template will render something like this:

```html
<div class="widget-container">
    <div class="widget-container-title">
        <h2>A Paragraph</h2>
    </div>
    <div class="widget widget-html-widget">
        <div class="widget-body">
            <p>This is a paragraph</p>
        </div>
    </div>
</div>
```

If the HTML contains `<div class="widget-container">` then your widget has been rendered by the `Layers` modules which will add this automatically, as it needs to 
be able to render a title, and uses it as a container for both the title and the widget's actual content.

The actual template for the `Widget` shape can be found in `src/OrchardCore.Modules/OrchardCore.Widgets/Views/Widgets.cshtml` but can be simplified to this:

=== "Liquid"

    ``` liquid
    <div class="{{ Model.Classes | join " "}}">
        <div class="widget-body">
        {{ Model.Content | shape_render }}
        </div>
    </div>
    ```

=== "Razor"

    ``` html
    <div class="@String.Join(" ", Model.Classes.ToArray())">
        <div class="widget-body">
        @await DisplayAsync(Model.Content)
        </div>
    </div>
    ```

Alternates are available per Content Type.

| Definition | Template | Filename|
| ---------- | --------- | ------------ |
| `Widget__[ContentType]` | `Widget__Paragraph` | `Widget-Paragraph.cshtml` |

### Customizing the `Widget_Wrapper` template

As mentioned in the previous section, the `Layers` module uses a template to wrap the widgets that it renders and insert a custom title for each of them.

The actual template for this wrapper shape can be found in `src/OrchardCore.Modules/OrchardCore.Layers/Views/Widget.Wrapper.cshtml`.

A common requirement is to remove these tags, which can be done by creating this template instead:

=== "Liquid"

    ``` liquid
    {{ Model.Content | shape_render }}
    ```

=== "Razor"

    ``` html
    @await DisplayAsync(Model.Content)
    ```

Optionally, you can change these alternates:

| Definition | Template | Filename|
| ---------- | --------- | ------------ |
| `Widget_Wrapper__[ContentType]` | `Widget_Wrapper__Paragraph` | `Widget-Paragraph.Wrapper.cshtml` |
| `Widget_Wrapper__Zone__[ContentZone]` | `Widget_Wrapper__Zone__Footer` | `Widget-Zone-Footer.Wrapper.cshtml` |
