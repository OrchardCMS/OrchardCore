# Widgets (OrchardCore.Widgets)

## Theming

These alternates or templates are available for theming a `Widget`, the `Widget_Wrapper` and a widget using its type.

### Widget

By default, it inherits from a Content some it is displayed as an `<article>`.

```liquid
{{ Model.Content | shape_render }}
```

```razor
@await DisplayAsync(Model.Content)
```

### Widget Wrapper

By default, it contains the code to render or not the Title.

```liquid
{{ Model.Content | shape_render }}
```

```razor
@await DisplayAsync(Model.Content)
```

### Widget by type

Use `Widget__{WidgetType}` to override the template of each widget depending on its type. 
Ex: `Widget__RawHtml`

```liquid
{% assign fields = Model.ContentItem.Content[Model.ContentItem.ContentType] %}
{{ fields.Content.Html | raw }}
```
