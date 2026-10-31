# Templates (`OrchardCore.Templates`)

The templates module allows editors to create custom Liquid templates.

## Assisted Templates

The **Assisted Templates** feature (`OrchardCore.Templates.Assisted`) adds the tools below to the **Templates** page, and to the **Admin Templates** page when the **Admin Templates** feature is enabled: the **Theme Templates** and **Feature Templates** lists. It is not enabled by default: enable it in **Configuration > Features**.

The **Shape Tracing** feature (`OrchardCore.Templates.ShapeTracing`) adds the tools that need the content items, and depends on the **Contents** feature: the [alternates](#alternates) suggested for the content types, parts and fields of the site, and [Inspect Content](#inspect-content). It is not enabled by default either.

## Theme Templates

The **Theme Templates** button of the **Design > Templates** page lists the templates of the site themes, and the same button of the **Design > Admin Templates** page those of the admin themes. A dropdown selects the theme whose templates are listed, the active theme by default; the templates of a theme include those of the themes it is based on. Each template is shown with the file it comes from, e.g. `TheBlogTheme` `Views/Content-BlogPost.liquid`, and the name of the template that overrides it, e.g. `Content__BlogPost`.

From the list of the active theme, without changing the theme, you can:

- **View** the content of any template of the theme.
- **Clone** a Liquid template as a local template with the same name, which then overrides it, and **Edit** it like any other template.
- **Reset** a local template to the template of the theme, which replaces its content.
- **Delete** a local template, so that the template of the theme is used again.

The same actions are available as bulk actions on the selected templates.

Razor (`.cshtml`) templates can only be viewed, as local templates are written in Liquid: use them as a reference to write a Liquid template with the same name, with the **Create override** button.

The templates of a theme that is not active can only be viewed: a local template applies at once to the active theme, so cloning the template of another theme would render the active theme with it. Select the theme on the **Themes** page first.

The status of each template shows whether it is overridden, and monitors the templates cloned from this list:

| Status | Description |
|--------|-------------|
| Not overridden | The template of the theme is used. |
| Cloned | The local template is the same as the template it was cloned from. |
| Edited | The local template was edited since it was cloned. |
| Overridden | A local template overrides it, but it was not cloned from this list, so its changes are not monitored. |
| Cloned from _theme_ | A local template overrides it, cloned from the template of another theme, e.g. the theme that was active before. It is not compared to this template: review it, reset it to this template, or delete it. |
| Source changed | The template of the theme changed since it was cloned, e.g. after the theme was updated. Review it, or reset the local template to it. |

The list can be filtered by these statuses, and by language with the **Liquid** and **Razor** options, in the **Theme Templates** and **Feature Templates** lists. The clone records are stored by tenant next to the templates, but are not part of their recipes and deployments, so the templates imported in another tenant are shown as **Overridden**.

## Feature Templates

The **Feature Templates** button, next to **Theme Templates**, lists the templates of the enabled features, all of them by default, or those of the feature selected in the dropdown. They are almost all Razor templates, so they can be viewed as a reference to write a Liquid template with the same name, with the **Create override** button, which shows the name of each shape to override.

A template that is not the one rendering its shape is shown as **Overridden by** the extension whose template renders it: the active theme, which overrides the templates of the features, or another feature, the last one enabled winning. A local template with the same name overrides both.

Features don't tell which of their templates render the admin, so the list guesses it from the names of the templates, e.g. `TextField_Edit`, `Content_SummaryAdmin` or the workflow activities ending with `_Design`, and filters them with the **Renders** filter: the front end on the **Templates** page and the admin on the **Admin Templates** page by default, or both.

## Alternates

An alternate is a more specific name of a shape, which renders it only in some cases, e.g. `Content_Summary__BlogPost` renders the `Content` shape of the blog posts in the `Summary` display type, while `Content` keeps rendering the others. The **Alternates** button of a template in the **Theme Templates** and **Feature Templates** lists, and of the page viewing a template, shows the alternates of its shape for the content types of the site, when it has some: those of the content items (`Content`, or the stereotype of the content type, e.g. `Widget`), of their parts and of their fields, in the `Detail` and `Summary` display types on the **Templates** page, and in the `DetailAdmin` and `SummaryAdmin` display types on the **Admin Templates** page. They are the alternates the content display drivers add, described in [Available templates](#available-templates).

Each alternate shows whether a local template has its name, and whether the theme or a feature already has a template rendering it. Its **Create** button opens the editor of a new template with this name. When the template of the list is a Liquid template of the active theme, its **Clone** button creates the template with its content, as a starting point; when the theme already has a template for the alternate, its **View** button shows this template, to clone it instead.

Modules can suggest the alternates of other shapes by registering an `ITemplateAlternatesProvider`.

## Inspect Content

The **Inspect Content** button of the **Templates** and **Admin Templates** pages, and the **Inspect templates** action of the content items of the **Content Items** list, show the shapes a content item renders in a display type: each shape, indented under the shape rendering it, with the template that renders it and all its alternates, the most specific first, including those of this content item only, e.g. `Content__Alias__[Alias]` or `Content__Slug__[Slug]`. The alternate the template is bound to is checked, and each alternate opens the editor of the local template with its name.

The content items to inspect are picked among the 50 most recently modified ones that the user can view, with the **View content** permission. The search box filters them by their display text, to find an older one.

When a template of the active theme or of a feature renders the shape, or is bound to one of its alternates, **View** shows its content and **Clone** copies it as a local template, for a Liquid template, to customize it. A Razor template can only be viewed, and its alternate creates a new Liquid template with its name.

The templates are resolved as the front end does, with the active site theme and the templates, on the **Templates** page, and as the admin does, with the active admin theme and the admin templates, on the **Admin Templates** page. The content item is rendered in the admin to find its shapes, so the shapes that a template of the site theme renders in its own markup are not listed.

### Template sources

Local templates are not tied to a theme: when the active theme changes, the templates cloned from the previous theme keep overriding the shapes of the new one. They are shown as **Cloned from** the previous theme.

The templates are listed by template sources, each shown as a button of the templates list. A source can have scopes, e.g. the themes of the theme templates, selected with the dropdown. A module can list the templates of other extensions by registering an `ITemplateSource`:

```csharp
public sealed class MyTemplateSource : ITemplateSource
{
    private readonly ShapeTemplateFiles _shapeTemplateFiles;

    public MyTemplateSource(ShapeTemplateFiles shapeTemplateFiles, IStringLocalizer<MyTemplateSource> S)
    {
        _shapeTemplateFiles = shapeTemplateFiles;
        DisplayName = S["My Templates"];
    }

    // The name in the URL of the list, e.g. '/Admin/TemplateSources/My'.
    public string Name => "My";

    public LocalizedString DisplayName { get; }

    public LocalizedString GetDescription(bool adminTemplates) => DisplayName;

    // No scopes: the templates are listed at once.
    public Task<IEnumerable<TemplateSourceScope>> GetScopesAsync(bool adminTemplates)
        => Task.FromResult(Enumerable.Empty<TemplateSourceScope>());

    public async Task<IEnumerable<SourceTemplate>> GetTemplatesAsync(bool adminTemplates, string scope)
    {
        // The templates rendering the shapes of a theme, by the theme or an enabled module.
        var templates = await _shapeTemplateFiles.GetTemplatesAsync("TheTheme");

        return templates.Where(template => template.ExtensionId == "My.Module");
    }
}
```

```csharp
services.AddScoped<ITemplateSource, MyTemplateSource>();
```

## Available templates

Templates can be defined using the web editor, or in a theme. Templates are distinguished by their name.  
Orchard Core doesn't render HTML directly, but instead will usually render something called a **Shape**, which is an object that represents
the thing to render and has all the necessary data and metadata to render HTML.

When rendering a Shape, Orchard Core will look for specific templates, passing the Shape to this template.  
Orchard Core can match with many templates for the same Shape.  
These potential templates are called **Alternates**.  
A Shape contains a list of acceptable template names (the alternates) and will look into providers to get the most appropriate one.  
For instance, when rendering a Content Item of type Article, the corresponding Shape that is rendered will be configured
to look for a template that handles all articles, but also one that handles an article when used in a list, and so on.

This document provides a list of pre-defined templates that can be used when rendering shapes.  
It uses the internal name of a template and also the filename in case it's provided by a Theme.

## Content templates

### `Content__[ContentType]`

This template is called when displaying a content item with the `Detail` display type, for instance when accessed from its own URL.

#### Content Examples

| Template            | Filename                  |
|---------------------|---------------------------|
| `Content__BlogPost` | `Content-BlogPost.cshtml` |
| `Content__Article`  | `Content-Article.cshtml`  |

#### Content Available properties

| Property                    | Description                                                                            |
|-----------------------------|----------------------------------------------------------------------------------------|
| `Model.Content`             | A zone shape that contains all the shapes generated by the content's parts and fields. |
| `Model.ContentItem`         | Represents the current content item being rendered by the template.                    |
| `Model.ContentItem.Content` | A JSON object containing all the data of the content item.                             |

### `Content_[DisplayType]__[ContentType]`

This template is called when displaying a content item with a specific display type.
For instance, when a content item is displayed in a list, the `Summary` display type is commonly used.

#### Content with Display Type Examples

| Template                    | Filename                          |
|-----------------------------|-----------------------------------|
| `Content_Summary__BlogPost` | `Content-BlogPost.Summary.cshtml` |
| `Content_Summary__Article`  | `Content-Article.Summary.cshtml`  |

### `Content__Alias__[Alias]`

This template is called when displaying a content item with a specific alias. It needs to have the `AliasPart` attached to it.

#### Content with Alias Examples

| Template                   | Filename                       |
|----------------------------|--------------------------------|
| `Content__Alias__example`  | `Content-Alias-example.cshtml` |
| `Content__Alias__my__page` | `Content-Alias-my-page.cshtml` |

#### Content with Alias and Display Type Examples

| Template                           | Filename                               |
|------------------------------------|----------------------------------------|
| `Content_Summary__Alias__example`  | `Content-Alias-example.Summary.cshtml` |
| `Content_Summary__Alias__my__page` | `Content-Alias-my-page.Summary.cshtml` |

### `Content__Slug__[Slug]`

This template is called when displaying a content item with a specific slug (i.e., a path is assigned to the item). It needs to have the `AutoroutePart` attached to it.

#### Content with Slug Examples

| Template                        | Filename                           |
|---------------------------------|------------------------------------|
| `Content__Slug__example`        | `Content-Slug-example.cshtml`      |
| `Content__Slug__blog__my__post` | `Content-Slug-blog-my-post.cshtml` |

#### Content with Slug and Display Type Examples

| Template                                | Filename                                   |
|-----------------------------------------|--------------------------------------------|
| `Content_Summary__Slug__example`        | `Content-Slug-example.Summary.cshtml`      |
| `Content_Summary__Slug__blog__my__post` | `Content-Slug-blog-my-post.Summary.cshtml` |

## Widget templates

### `Widget__[ContentType]`

This template is called when a widget is rendered on a page.

#### Widget Examples

| Template             | Filename                   |
|----------------------|----------------------------|
| `Widget__Paragraph`  | `Widget-Paragraph.cshtml`  |
| `Widget__Blockquote` | `Widget-Blockquote.cshtml` |

#### Widget Available properties

| Property                    | Description                                                                           |
|-----------------------------|---------------------------------------------------------------------------------------|
| `Model.Content`             | A zone shape that contains all the shapes generated by the widget's parts and fields. |
| `Model.ContentItem`         | Represents the current content item being rendered by the template.                   |
| `Model.ContentItem.Content` | A JSON object containing all the data of the content item.                            |
| `Model.Classes`             | An array of all the classes attached to the widget.                                   |

### `Widget__Alias__[Alias]`

This template is called when displaying a widget content item with a specific alias. It needs to have the `AliasPart` attached to it.

#### Widget with Alias Examples

| Template                  | Filename                      |
|---------------------------|-------------------------------|
| `Widget__Alias__example`  | `Widget-Alias-example.cshtml` |
| `Widget__Alias__my__page` | `Widget-Alias-my-page.cshtml` |

#### Widget with Alias and Display Type Examples

| Template                          | Filename                              |
|-----------------------------------|---------------------------------------|
| `Widget_Summary__Alias__example`  | `Widget-Alias-example.Summary.cshtml` |
| `Widget_Summary__Alias__my__page` | `Widget-Alias-my-page.Summary.cshtml` |

### `Widget__Slug__[Slug]`

This template is called when displaying a widget content item with a specific slug (i.e., a path is assigned to the item). It needs to have the `AutoroutePart` attached to it.

#### Widget with Slug Examples

| Template                       | Filename                          |
|--------------------------------|-----------------------------------|
| `Widget__Slug__example`        | `Widget-Slug-example.cshtml`      |
| `Widget__Slug__blog__my__post` | `Widget-Slug-blog-my-post.cshtml` |

#### Widget with Slug and Display Type Examples

| Template                               | Filename                                  |
|----------------------------------------|-------------------------------------------|
| `Widget_Summary__Slug__example`        | `Widget-Slug-example.Summary.cshtml`      |
| `Widget_Summary__Slug__blog__my__post` | `Widget-Slug-blog-my-post.Summary.cshtml` |

## Content Part templates

Each driver is free to return a shape type of its choosing, but the usage is
to render a content part using a shape with a name that matches the type name.
For instance the `HtmlBodyPart` content part will return a single shape of type `HtmlBodyPart`, but the `ListPart` returns many
shapes, one among them being `ListPart`.

As a consequence the following list of templates use the `[ShapeType]` term where
most of the time it will be equal to the name of the content part. The examples use
common content part names for this reason.

### Properties

The properties available on a shape rendered for a content part are unique for each content
part. Please refer to the documentation for each content part.

### `[ShapeType]`

This template is called when a Content Part is rendered.

#### Shape Examples

| Template       | Filename              |
|----------------|-----------------------|
| `HtmlBodyPart` | `HtmlBodyPart.cshtml` |
| `ListPartFeed` | `ListPartFeed.cshtml` |

### `[ShapeType]_[DisplayType]`

This template is called when a Content Part shape type is rendered in a specific display type.

#### Shape with Display Type Examples

| Template               | Filename                      |
|------------------------|-------------------------------|
| `HtmlBodyPart_Summary` | `HtmlBodyPart.Summary.cshtml` |

### `[ContentType]_[DisplayType]__[PartType]`

This template is called when a content part type is rendered for a given content type, with or without a given display type.

#### Content with Part Type Examples

| Template                       | Filename                             |
|--------------------------------|--------------------------------------|
| `Blog__HtmlBodyPart`           | `Blog-HtmlBodyPart.cshtml`           |
| `LandingPage__BagPart`         | `LandingPage-BagPart.cshtml`         |
| `Blog_Summary__HtmlBodyPart`   | `Blog-HtmlBodyPart.Summary.cshtml`   |
| `LandingPage_Summary__BagPart` | `LandingPage-BagPart.Summary.cshtml` |

### `[ContentType]_[DisplayType]__[PartName]`

This template is called when a content part name is rendered for a given content type, with or without a given display type.

#### Content with Part name Examples

| Template                        | Filename                              |
|---------------------------------|---------------------------------------|
| `LandingPage__Services`         | `LandingPage-Services.cshtml`         |
| `LandingPage_Summary__Services` | `LandingPage-Services.Summary.cshtml` |

### `[ContentType]_[DisplayType]__[PartType]__[ShapeType]`

This template is called when a shape type is rendered in a given content part type for a given content type, with or without a given display type.

#### Content with Part Type and Shape Examples

| Template                               | Filename                                    |
|----------------------------------------|---------------------------------------------|
| `Blog__ListPart__ListPartFeed`         | `Blog-ListPart-ListPartFeed.cshtml`         |
| `Blog_Summary__ListPart__ListPartFeed` | `Blog-ListPart-ListPartFeed.Summary.cshtml` |

### `[ContentType]_[DisplayType]__[PartName]__[ShapeType]`

This template is called when a shape type is rendered in a given content part name for a given content type, with or without a given display type.

#### Content with Part Name and Custom Shape Examples

| Template                                     | Filename                                          |
|----------------------------------------------|---------------------------------------------------|
| `LandingPage__Services__CustomShape`         | `LandingPage-Services-CustomShape.cshtml`         |
| `LandingPage_Summary__Services__CustomShape` | `LandingPage-Services-CustomShape.Summary.cshtml` |

### Content Parts Display Mode

Content parts that supports different display modes, also provides following display mode alternates.

### `[ShapeType]_[DisplayType]__[DisplayMode]_Display`

This template is called when a content part is rendered for a given content part type, with given display mode, with a display type.

#### Display mode with Shape type Examples

| Template                                | Filename                                      |
|-----------------------------------------|-----------------------------------------------|
| `TitlePart_Summary__CustomMode_Display` | `TitlePart-CustomMode.Display.Summary.cshtml` |

### `[ContentType]_Display__[PartType]__[DisplayMode]`

This template is called when a content part is rendered for a given content type, with given display mode, without a display type.

#### Display mode with Part type without Display type Examples

| Template                                     | Filename                                          |
|----------------------------------------------|---------------------------------------------------|
| `LandingPage_Display__TitlePart__CustomMode` | `LandingPage-TitlePart-CustomMode.Display.cshtml` |

### `[ContentType]_[DisplayType]__[PartType]__[DisplayMode]_Display`

This template is called when a content part is rendered for a given content type, with given display mode, with a display type.

#### Display mode with Part type with Display type Examples

| Template                                             | Filename                                                  |
|------------------------------------------------------|-----------------------------------------------------------|
| `LandingPage_Summary__TitlePart__CustomMode_Display` | `LandingPage-TitlePart-CustomMode.Display.Summary.cshtml` |

### `[ContentType]_Display__[PartName]__[DisplayMode]`

This template is called when a content part name is rendered for a given content type, for given display mode, without a given display type.

#### Display mode with Part name without Display type Examples

| Template                                    | Filename                                         |
|---------------------------------------------|--------------------------------------------------|
| `LandingPage_Display__Services__CustomMode` | `LandingPage-Services-CustomMode.Display.cshtml` |

### `[ContentType]_[DisplayType]__[PartName]__[DisplayMode]_Display`

This template is called when a content part name is rendered for a given content type, for given display mode, with a given display type.

#### Display mode with Part name with Display type Examples

| Template                                            | Filename                                                 |
|-----------------------------------------------------|----------------------------------------------------------|
| `LandingPage_Summary__Services__CustomMode_Display` | `LandingPage-Services-CustomMode.Display.Summary.cshtml` |

### `[ContentType]_Display__[PartType]__[ShapeType]__[DisplayMode]`

This template is called when a shape type is rendered in a given content part type for a given content type, for given display mode without a given display type.

#### Display mode with Part Type and Shape without Display type Examples

| Template                                           | Filename                                               |
|----------------------------------------------------|--------------------------------------------------------|
| `Blog_Display__ListPart__ListPartFeed__CustomMode` | `Blog-ListPart-ListPartFeed-CustomMode.Display.cshtml` |

### `[ContentType]_[DisplayType]__[PartType]__[ShapeType]__[DisplayMode]_Display`

This template is called when a shape type is rendered in a given content part type for a given content type, for given display mode with a given display type.

#### Display mode with Part Type and Shape with Display type Examples

| Template                                                   | Filename                                                       |
|------------------------------------------------------------|----------------------------------------------------------------|
| `Blog_Summary__ListPart__ListPartFeed__CustomMode_Display` | `Blog-ListPart-ListPartFeed-CustomMode.Display.Summary.cshtml` |

### `[ContentType]_Display__[PartName]__[ShapeType]__[DisplayMode]`

This template is called when a shape type is rendered in a given content part name for a given content type, without a given display type.

#### Display mode with Part Name and Custom Shape without Display type Examples

| Template                                           | Filename                                               |
|----------------------------------------------------|--------------------------------------------------------|
| `Blog_Display__Services__ListPartFeed__CustomMode` | `Blog-Services-ListPartFeed-CustomMode.Display.cshtml` |

### `[ContentType]_[DisplayType]__[PartName]__[ShapeType]__[DisplayMode]_Display`

This template is called when a shape type is rendered in a given content part name for a given content type, with a given display type.

#### Display mode with Part Name and Custom Shape with Display type Examples

| Template                                                   | Filename                                                       |
|------------------------------------------------------------|----------------------------------------------------------------|
| `Blog_Summary__Services__ListPartFeed__CustomMode_Display` | `Blog-Services-ListPartFeed-CustomMode.Display.Summary.cshtml` |

### Widget parts / Stereotype parts

Content Part attached to Content type of the stereotype (e.g. `Widget`, `Menu`) also has following alternates. This applies to any stereotype other than `Content`.

### `[Stereotype]_[DisplayType]__[PartType]`

This template is called when a content part is rendered for a given stereo type, with or without a given display type.

#### Stereotype with Part Name Examples

| Template                       | Filename                             |
|--------------------------------|--------------------------------------|
| `Widget__HtmlBodyPart`         | `Widget-HtmlBodyPart.cshtml`         |
| `Widget_Summary__HtmlBodyPart` | `Widget-HtmlBodyPart.Summary.cshtml` |

### `[Stereotype]_[DisplayType]__[PartType]__[PartName]`

This template is called when a content part is a reusable named part and rendered for a given stereotype, with or without a given display type.

#### Stereotype with Part Type Examples

| Template                                | Filename                                     |
|-----------------------------------------|----------------------------------------------|
| `Widget__ServicePart__Services`         | `Widget-ServicePart-Services.cshtml`         |
| `Widget_Summary__ServicePart__Services` | `Widget-ServicePart-Services.Summary.cshtml` |

### `[Stereotype]_[DisplayType]__[PartType]__[ShapeType]`

This template is called when a custom shape type is rendered for a given stereotype type, with or without a given display type.

#### Stereotype with Part Name and Shape Examples

| Template                                 | Filename                                      |
|------------------------------------------|-----------------------------------------------|
| `Widget__ListPart__ListPartFeed`         | `Widget-ListPart-ListPartFeed.cshtml`         |
| `Widget_Summary__ListPart__ListPartFeed` | `Widget-ListPart-ListPartFeed.Summary.cshtml` |

### `[Stereotype]_[DisplayType]__[PartType]__[PartName]__[ShapeType]`

This template is called for a reusable named content part with a custom shape type and rendered for a given stereotype type, with or without a given display type.

#### Stereotype with Part Name and Custom Shape Examples

| Template                                             | Filename                                                 |
|------------------------------------------------------|----------------------------------------------------------|
| `Widget__ServicePart__Services__CustomShape`         | `Widget-ServicePart-Services-CustomShape.cshtml`         |
| `Widget_Summary__ServicePart__Services__CustomShape` | `Widget-ServicePart-Services-CustomShape.Summary.cshtml` |

Following alternates are available to Content parts that supports different display modes and are attached to content type of stereotype.

### `[Stereotype]_Display__[PartType]__[DisplayMode]`

This template is called when a content part is rendered for a given stereo type, with given display mode, without a display type.

#### Display mode with Part type without Display type Examples

| Template                                | Filename                                     |
|-----------------------------------------|----------------------------------------------|
| `Widget_Display__TitlePart__CustomMode` | `Widget-TitlePart-CustomMode.Display.cshtml` |

### `[Stereotype]_[DisplayType]__[PartType]__[DisplayMode]_Display`

This template is called when a content part is rendered for a given stereo type, with given display mode, with a display type.

#### Display mode with Stereotype with Display type Examples

| Template                                        | Filename                                             |
|-------------------------------------------------|------------------------------------------------------|
| `Widget_Summary__TitlePart__CustomMode_Display` | `Widget-TitlePart-CustomMode.Display.Summary.cshtml` |

### `[Stereotype]_Display__[PartType]__[PartName]__[DisplayMode]`

This template is called when a content part name is rendered for a given stereotype, for given display mode, without a given display type.

#### Display mode with Part name without Display type Examples

| Template                                            | Filename                                                |
|-----------------------------------------------------|---------------------------------------------------------|
| `Widget_Display__ServicePart__Services__CustomMode` | `Widget-ServicePart-Services-CustomMode.Display.cshtml` |

### `[Stereotype]_[DisplayType]__[PartType]__[PartName]__[DisplayMode]_Display`

This template is called when a content part name is rendered for a given stereotype, for given display mode, with a given display type.

#### Display mode with Part name with Display type Examples

| Template                                                    | Filename                                                        |
|-------------------------------------------------------------|-----------------------------------------------------------------|
| `Widget_Summary__ServicePart__Services__CustomMode_Display` | `Widget-ServicePart-Services-CustomMode.Display.Summary.cshtml` |

### `[Stereotype]_Display__[PartType]__[ShapeType]__[DisplayMode]`

This template is called when a shape type is rendered in a given content part type for a given stereotype, for given display mode without a given display type.

#### Display mode with Part Type and Shape without Display type Examples

| Template                                             | Filename                                                 |
|------------------------------------------------------|----------------------------------------------------------|
| `Widget_Display__ListPart__ListPartFeed__CustomMode` | `Widget-ListPart-ListPartFeed-CustomMode.Display.cshtml` |

### `[Stereotype]_[DisplayType]__[PartType]__[ShapeType]__[DisplayMode]_Display`

This template is called when a shape type is rendered in a given content part type for a given stereotype, for given display mode with a given display type.

#### Display mode with Part Type and Shape with Display type Examples

| Template                                                     | Filename                                                         |
|--------------------------------------------------------------|------------------------------------------------------------------|
| `Widget_Summary__ListPart__ListPartFeed__CustomMode_Display` | `Widget-ListPart-ListPartFeed-CustomMode.Display.Summary.cshtml` |

### `[Stereotype]_Display__[PartType]__[PartName]__[ShapeType]__[DisplayMode]`

This template is called when a shape type is rendered in a given content part name for a given stereotype, without a given display type.

#### Display mode with Part Name and Custom Shape without Display type Examples

| Template                                                       | Filename                                                          |
|----------------------------------------------------------------|-------------------------------------------------------------------|
| `Widget_Display__ListPart__Services__ListPartFeed__CustomMode` | `Widget-ListPart-Services-ListPartFeed-CustomMode.Display.cshtml` |

### `[Stereotype]_[DisplayType]__[PartType]__[PartName]__[ShapeType]__[DisplayMode]_Display`

This template is called when a shape type is rendered in a given content part name for a given stereotype, with a given display type.

#### Display mode with Part Name and Custom Shape with Display type Examples

| Template                                                               | Filename                                                                  |
|------------------------------------------------------------------------|---------------------------------------------------------------------------|
| `Widget_Summary__ListPart__Services__ListPartFeed__CustomMode_Display` | `Widget-ListPart-Services-ListPartFeed-CustomMode.Display.Summary.cshtml` |

## Content Field templates

Each driver is free to return a shape type of its choosing but the usage is
to render a content field using a shape with the same type name.
For instance the `TextField` content field will return a single shape of type `TextField`,
but other fields might return many shapes.

### Shape Properties

The properties available on a shape rendered for a content field are unique for each content
field. Please refer to each content field documentation.

### `[ShapeType]_[DisplayType]` Field

This template is called when a content field type is rendered in a given display type.

#### Field And Display Type Examples

| Template            | Filename                   |
|---------------------|----------------------------|
| `TextField_Summary` | `TextField.Summary.cshtml` |

### `[PartType]__[FieldName]`

This template is called when a content field name is rendered for a given content part type when the shape type matches the field type, with or without a given display type.

#### Part Type and Field Examples

| Template                            | Filename                                  |
|-------------------------------------|-------------------------------------------|
| `HtmlBodyPart__Description`         | `HtmlBodyPart-Description.cshtml`         |
| `HtmlBodyPart_Summary__Description` | `HtmlBodyPart-Description.Summary.cshtml` |

### `[ContentType]__[PartName]__[FieldName]`

This template is called when a content field name is rendered for a given content type and content part name when the shape type matches the field type, with or without a given display type.

#### Content Type, Part Name and Field Name Examples

| Template                                  | Filename                                       |
|-------------------------------------------|------------------------------------------------|
| `Blog__HtmlBodyPart__Description`         | `Blog-HtmlBodyPart-Description.cshtml`         |
| `LandingPage__Services__Image`            | `LandingPage-Services-Image.cshtml`            |
| `Blog_Summary__HtmlBodyPart__Description` | `Blog-HtmlBodyPart-Description.Summary.cshtml` |
| `LandingPage_Summary__Services__Image`    | `LandingPage-Services-Image.Summary.cshtml`    |

### `[ContentType]__[FieldType]`

This template is called when a content field type is rendered for a given content type when the shape type matches the field type, with or without a given display type.

#### Content Type and Field Type Examples

| Template                 | Filename                       |
|--------------------------|--------------------------------|
| `Blog__TextField`        | `Blog-TextField.cshtml`        |
| `LandingPage__TextField` | `LandingPage-TextField.cshtml` |

### `[FieldType]__[ShapeType]`

This template is called when a content field shape type is rendered for a given content field type, with or without a given display type.

#### Field Type and Shape Type Examples

| Template                                  | Filename                                        |
|-------------------------------------------|-------------------------------------------------|
| `CustomField__CustomFieldSummary`         | `CustomField-CustomFieldSummary.cshtml`         |
| `CustomField_Summary__CustomFieldSummary` | `CustomField-CustomFieldSummary.Summary.cshtml` |

### `[PartType]__[FieldName]__[ShapeType]`

This template is called when a content field shape type is rendered for a given content field name in a given content part type, with or without a given display type.

#### Part Type, Field Name and Shape Type Examples

| Template                                                | Filename                                                       |
|---------------------------------------------------------|----------------------------------------------------------------|
| `HtmlBodyPart__Description__CustomFieldSummary`         | `HtmlBodyPart__Description__CustomFieldSummary.cshtml`         |
| `HtmlBodyPart_Summary__Description__CustomFieldSummary` | `HtmlBodyPart__Description__CustomFieldSummary.Summary.cshtml` |

### `[ContentType]__[PartName]__[FieldName]__[ShapeType]`

This template is called when a content field shape type is rendered for a given content field name in a given content part name in a given content type, with or without a given display type.

#### Content Type, Part Name, Field Name and Shape Type Examples

| Template                                                         | Filename                                                             |
|------------------------------------------------------------------|----------------------------------------------------------------------|
| `Blog__HtmlBodyPart__Description__CustomFieldSummary`            | `Blog-HtmlBodyPart-Description-CustomFieldSummary.cshtml`            |
| `LandingPage__Services__Description__CustomFieldSummary`         | `LandingPage-Services-Description-CustomFieldSummary.cshtml`         |
| `Blog_Summary__HtmlBodyPart__Description__CustomFieldSummary`    | `Blog-HtmlBodyPart-Description-CustomFieldSummary.Summary.cshtml`    |
| `LandingPage_Summary__Services__Description__CustomFieldSummary` | `LandingPage-Services-Description-CustomFieldSummary.Summary.cshtml` |

### `[ContentType]__[FieldType]__[ShapeType]`

This template is called when a content field shape type is rendered for a given content field type in a given content type, with or without a given display type.

#### Content Type, Field Type and Shape Type Examples

| Template                                   | Filename                                        |
|--------------------------------------------|-------------------------------------------------|
| `Blog__TextField__TextFieldSummary`        | `Blog-TextField-TextFieldSummary.cshtml`        |
| `LandingPage__TextField__TextFieldSummary` | `LandingPage-TextField-TextFieldSummary.cshtml` |

## Shape differentiators

The differentiator uniquely identifies a shape in a zone. When rendering a content item, the shape has a `Content` property that contains
all the shapes provided by content display drivers, including the ones for content parts and content fields.

Differentiators can be used to configure the placement information (c.f. [Placement documentation page](../../modules/Placement/README.md)), or to access specific shapes in a zone using these template helpers:

### Content Part differentiator

If the shape type is the same as the content part name, the shape will be named `[PartName]`, e.g. `HtmlBodyPart`, `Services`.  
If the shape type is different than the content part name, it will be `[PartName]-[ShapeType]`, e.g. `ListPart-ListPartFeed`

### Content Field differentiator

If the shape type is the same as the content field name, the shape will be named `[PartName]-[FieldName]`, e.g. `HtmlBodyPart-Description`, `Services-Image`.  
If the shape type is different than the content field name, it will be `[PartName]-[FieldName]-[ShapeType]`, e.g. `HtmlBodyPart-Description-CustomFieldSummary`, `Services-Image-ImageFieldSummary`

#### Razor

Access a specific shape by name:

```csharp
Model.Content.HtmlBodyPart;
Model.Content.Named("ListPart-ListPartFeed");
```

Removing a specific shape by name:

```csharp
Model.Content.Remove("HtmlBodyPart");
```

#### Liquid

Display a shape after removing a specific shape by name:

```liquid
{% shape_remove_item Model.Content "HtmlBodyPart" %}
{{ Model.Content | shape_render }}
```

Display a specific shape by name:

```liquid
{{ Model.Content.HtmlBodyPart | shape_render }}
```

To access or render the shapes for a field that is added to the content type directly, the `[PartName]` is equal to the content type.  
For instance, given a content type `Article` with a Text field named `Description`, the shapes for this field would be named `"Article-Description"`. To render these shapes in Liquid:

```liquid
{{ Model.Content["Article-Description"] | shape_render }}
```

In this example the index syntax is necessary, because the name of the differentiator is not compatible with Liquid language.

Instead of rendering the shape directly, you can also access its properties. In the case of a text field you have access to the `Field` property which has a `Text` property.

```liquid
{{ Model.Content["Article-Description"].Field.Text }}
```

## Zone templates

### `Zone__[ZoneName]`

This template is called when displaying a Layout zone.

#### Zone Examples

| Template        | Filename              |
|-----------------|-----------------------|
| `Zone__Footer`  | `Zone-Footer.cshtml`  |
| `Zone__Content` | `Zone-Content.cshtml` |

Which zones are available depends on the current theme.

!!! note
    The `Zone__Content` alternate is for the Layout zone called "Content", which (depending on the theme) is usually the main content area of the page. It has nothing to do with content item shapes.

#### Zone Available properties

| Property         | Description                                                                                                                                                                                                                               |
|------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Model.Items`    | While the property is not unique to zone shapes keep in mind that `Model.Items` contains the child shapes that should be displayed from the zone. In Razor but not in Liquid iterating over `Model` directly yields the child shapes too. |
| `Model.Parent`   | A zone shape that's the parent of the current zone. For root-level zones this will be the Layout shape.                                                                                                                                   |
| `Model.ZoneName` | The `string` name of the zone, e.g. "Footer" and "Content".                                                                                                                                                                               |

=== "Liquid"

    ``` liquid
    {% for shape in Model.Items %}
        {{ shape | shape_render }}
    {% endfor %}
    ```

=== "Razor"

    ``` csharp
    @foreach (var shape in Model)
    {
        @await DisplayAsync(shape);
    }
    ```

## User templates

### `UserDisplayName_[DisplayType]`

This template is called when displaying a User Name.

#### User Examples

| Template                                | Filename                                      |
|-----------------------------------------|-----------------------------------------------|
| `UserDisplayName_SummaryAdmin__johndoe` | `UserDisplayName-johndoe.SummaryAdmin.cshtml` |
| `UserDisplayName_SummaryAdmin`          | `UserDisplayName.SummaryAdmin.cshtml`         |

### Customizing User Display in Admin Lists

The `UserDisplayName` shape provides a flexible way to display user information throughout OrchardCore, particularly in admin lists. This shape is commonly used when displaying content authors, audit trail users, or any user reference.

#### Creating a UserDisplayName Shape

=== "Razor"

    Use the `<user-display-name>` tag helper to render user information:

    ```html
    <user-display-name 
        user-name="@(contentItem.Author)" 
        display-type="SummaryAdmin"
        cache-id="user-display-name-author" 
        title="@T["Author"].Value" />
    ```

=== "Liquid"

    Use the `shape_new` filter to create a UserDisplayName shape:

    ```liquid
    {% assign user_display_name = "UserDisplayName" | shape_new: user_name: contentItem.Author, display_type: "SummaryAdmin", cache_id: "user-display-name-author", title: "Author" %}
    {{ user_display_name | shape_render }}
    ```

    Or create and render in a single line:

    ```liquid
    {{ "UserDisplayName" | shape_new: user_name: contentItem.Author, display_type: "SummaryAdmin" | shape_render }}
    ```

##### Tag Helper Parameters

| Parameter                | Description                                                     | Required                                                                           |
|--------------------------|-----------------------------------------------------------------|------------------------------------------------------------------------------------|
| `user-name`              | The username to display                                         | Yes                                                                                |
| `display-type`           | The display type for template resolution (e.g., `SummaryAdmin`) | No                                                                                 |
| `cache-id`               | Cache identifier for the rendered output                        | No (defaults to `"user-display-name"`)                                             |
| `title`                  | Tooltip text to display on hover                                | No                                                                                 |
| `cache-tag`              | Cache tags for cache invalidation                               | No (automatically includes `user-display-name` and `user-display-name:{username}`) |
| `cache-context`          | Cache context for cache variation                               | No (automatically includes `username-{username}`)                                  |
| `cache-fixed-duration`   | Fixed cache duration (e.g., `"00:05:00"` for 5 minutes)         | No                                                                                 |
| `cache-sliding-duration` | Sliding cache duration                                          | No                                                                                 |

#### Shape Morphing

The `UserDisplayName` shape automatically morphs into two sub-shapes:

1. **`UserDisplayNameIcon`** - Renders the user icon
2. **`UserDisplayNameText`** - Renders the username text

This morphing behavior allows you to customize the icon and text independently by creating specific template overrides.

#### Default Templates

##### `UserDisplayName.SummaryAdmin.cshtml` / `UserDisplayName.SummaryAdmin.liquid`

=== "Razor"

    ```cshtml
    <span class="badge ta-badge fw-normal" data-bs-toggle="tooltip" title="@Model.Title">
        @await DisplayAsAsync(Model, "UserDisplayNameIcon")
        @await DisplayAsAsync(Model, "UserDisplayNameText")
    </span>
    ```

=== "Liquid"

    ```liquid
    <span class="badge ta-badge fw-normal" data-bs-toggle="tooltip" title="{{ Model.Title }}">
        {{ "UserDisplayNameIcon" | shape_new | shape_render }}
        {{ "UserDisplayNameText" | shape_new | shape_render }}
    </span>
    ```

##### `UserDisplayNameIcon.cshtml` / `UserDisplayNameIcon.liquid`

=== "Razor"

    ```cshtml
    <i class="fa-solid fa-user text-secondary" aria-hidden="true"></i>
    ```

=== "Liquid"

    ```liquid
    <i class="fa-solid fa-user text-secondary" aria-hidden="true"></i>
    ```

##### `UserDisplayNameText.cshtml` / `UserDisplayNameText.liquid`

=== "Razor"

    ```cshtml
    @Model.UserName
    ```

=== "Liquid"

    ```liquid
    {{ Model.UserName }}
    ```

##### `UserDisplayName.cshtml` / `UserDisplayName.liquid` (Base template)

=== "Razor"

    ```cshtml
    @await DisplayAsAsync(Model, "UserDisplayNameIcon")
    @await DisplayAsAsync(Model, "UserDisplayNameText")
    ```

=== "Liquid"

    ```liquid
    {{ "UserDisplayNameIcon" | shape_new | shape_render }}
    {{ "UserDisplayNameText" | shape_new | shape_render }}
    ```

#### Available Shape Properties

The `UserDisplayName` shape exposes the following properties:

| Property         | Type     | Description                                                           |
|------------------|----------|-----------------------------------------------------------------------|
| `Model.UserName` | `string` | The username of the user to display                                   |
| `Model.Title`    | `string` | The title/tooltip text (typically used in the HTML `title` attribute) |

!!! note
    The shape uses a dynamic `IShape` type, not a strongly-typed model. Additional properties can be accessed dynamically.

#### Customization Examples

##### Custom Icon Template

Create `UserDisplayNameIcon.cshtml` or `UserDisplayNameIcon.liquid` in your theme to customize the icon:

=== "Razor"

    ```cshtml
    <i class="fa-solid fa-user-circle text-primary" aria-hidden="true"></i>
    ```

=== "Liquid"

    ```liquid
    <i class="fa-solid fa-user-circle text-primary" aria-hidden="true"></i>
    ```

##### Loading User Object to Access Custom Properties

To access the full user object with custom properties and content parts:

=== "Razor"

    Inject `UserManager<IUser>`:

    ```cshtml
    @using Microsoft.AspNetCore.Identity
    @using OrchardCore.Users
    @using OrchardCore.Users.Models

    @inject UserManager<IUser> UserManager

    @{
        var user = await UserManager.FindByNameAsync((string)Model.UserName);
        if (user != null)
        {
            // Access standard user properties
            var email = user.Email;
            var userId = user.UserId;
            
            // Access custom content parts if the user is a content item
            if (user is User userContent)
            {
                // Example: Access a custom ProfilePart with FirstName and LastName fields
                // Replace ProfilePart with your actual custom part name
                var firstName = userContent.Content.ProfilePart?.FirstName?.Text;
                var lastName = userContent.Content.ProfilePart?.LastName?.Text;
            }
        }
    }
    ```

=== "Liquid"

    Use the `users_by_name` filter to load the user:

    ```liquid
    {% assign user = Model.UserName | users_by_name %}
    {% if user %}
        {# Access standard user properties #}
        {{ user.Email }}
        {{ user.UserId }}
        
        {# Access custom content parts if available #}
        {# Example: Access a custom ProfilePart with FirstName and LastName fields #}
        {# Replace ProfilePart with your actual custom part name #}
        {% assign first_name = user.Content.ProfilePart.FirstName.Text %}
        {% assign last_name = user.Content.ProfilePart.LastName.Text %}
    {% endif %}
    ```

##### Displaying Full Name Instead of Username

Create `UserDisplayNameText.cshtml` or `UserDisplayNameText.liquid` to display a full name:

=== "Razor"

    ```cshtml
    @using Microsoft.AspNetCore.Identity
    @using OrchardCore.Users
    @using OrchardCore.Users.Models

    @inject UserManager<IUser> UserManager

    @{
        var user = await UserManager.FindByNameAsync((string)Model.UserName);
        var displayName = Model.UserName;
        
        if (user is User userContent)
        {
            // Example: Access a custom ProfilePart with FirstName and LastName fields
            // Replace ProfilePart with your actual custom part name
            var firstName = userContent.Content.ProfilePart?.FirstName?.Text;
            var lastName = userContent.Content.ProfilePart?.LastName?.Text;
            
            if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
            {
                displayName = $"{firstName} {lastName}".Trim();
            }
        }
    }
    @displayName
    ```

=== "Liquid"

    ```liquid
    {% assign user = Model.UserName | users_by_name %}
    {% assign display_name = Model.UserName %}

    {% if user %}
        {# Example: Access a custom ProfilePart with FirstName and LastName fields #}
        {# Replace ProfilePart with your actual custom part name #}
        {% assign first_name = user.Content.ProfilePart.FirstName.Text %}
        {% assign last_name = user.Content.ProfilePart.LastName.Text %}
        
        {% if first_name != blank or last_name != blank %}
            {% assign display_name = first_name | append: " " | append: last_name | strip %}
        {% endif %}
    {% endif %}

    {{ display_name }}
    ```

##### Per-User Template Override

Create a user-specific template for a particular username (e.g., for user "admin"):

=== "Razor"

    **Filename:** `UserDisplayName-admin.SummaryAdmin.cshtml`

    ```cshtml
    <span class="badge ta-badge fw-normal text-danger" data-bs-toggle="tooltip" title="@Model.Title">
        <i class="fa-solid fa-user-shield" aria-hidden="true"></i>
        <strong>@Model.UserName</strong>
    </span>
    ```

=== "Liquid"

    **Filename:** `UserDisplayName-admin.SummaryAdmin.liquid`

    ```liquid
    <span class="badge ta-badge fw-normal text-danger" data-bs-toggle="tooltip" title="{{ Model.Title }}">
        <i class="fa-solid fa-user-shield" aria-hidden="true"></i>
        <strong>{{ Model.UserName }}</strong>
    </span>
    ```

#### Template Priority (Alternates Resolution Order)

Templates are resolved in the following priority order (most specific to least specific):

1. `UserDisplayName_[DisplayType]__[UserName]` (e.g., `UserDisplayName-johndoe.SummaryAdmin.cshtml` or `.liquid`)
2. `UserDisplayName_[DisplayType]` (e.g., `UserDisplayName.SummaryAdmin.cshtml` or `.liquid`)
3. `UserDisplayName` (base template: `UserDisplayName.cshtml` or `.liquid`)

When customizing sub-shapes:

- `UserDisplayNameIcon_[DisplayType]` (e.g., `UserDisplayNameIcon.SummaryAdmin.cshtml` or `.liquid`)
- `UserDisplayNameIcon` (e.g., `UserDisplayNameIcon.cshtml` or `.liquid`)
- `UserDisplayNameText_[DisplayType]` (e.g., `UserDisplayNameText.SummaryAdmin.cshtml` or `.liquid`)
- `UserDisplayNameText` (e.g., `UserDisplayNameText.cshtml` or `.liquid`)

OrchardCore will use the first matching template it finds.

#### Performance Considerations

##### Caching Behavior

The `UserDisplayName` tag helper automatically configures caching with:

- **Default cache-id**: `"user-display-name"`
- **Automatic cache tags**: `"user-display-name"` and `"user-display-name:{username}"`
- **Automatic cache context**: `"username-{username}"`

This ensures that:
- User displays are cached for better performance
- Cache is invalidated per-user when user data changes
- Multiple instances of the same username can share cached output

You can override these defaults or add additional cache configuration via tag helper parameters.

##### Database Query Implications

!!! warning
    Loading the full user object via `UserManager.FindByNameAsync()` in Razor templates or the `users_by_name` filter in Liquid templates will execute a database query **for each user display**, leading to the N+1 query problem. When displaying lists with many users:
    
    - The default templates only use the `UserName` property (no database query)
    - Custom templates that load the user object should be used carefully to avoid performance degradation
    - Consider using shape table events or content handlers to pre-load user data in bulk
    - Rely on caching to minimize repeated queries for the same users

#### Common Use Cases

1. **Display content author in list views** - Show who created or modified content items
2. **Audit trail user display** - Display user information in activity logs
3. **Custom user badges** - Add role-based or status-based visual indicators
4. **User avatars** - Replace the default icon with user profile pictures
5. **Full name display** - Show user's full name instead of username for better UX
6. **Administrative user highlighting** - Apply special styling for admin or system users
7. **User reputation or level indicators** - Display user rank, points, or achievements

#### Implementation Details

- **Tag Helper**: `src/OrchardCore/OrchardCore.DisplayManagement/TagHelpers/UserDisplayNameTagHelper.cs`
- **Shape Table Provider**: `src/OrchardCore/OrchardCore.Users.Core/Services/UserDisplayNameShapeTableProvider.cs`
- **Default Templates**: `src/OrchardCore.Modules/OrchardCore.Users/Views/UserDisplayName*.cshtml`

## Overriding Views

Some modules (namely the OrchardCore.Users module) allow you to override some of its views in your Theme. Since these views are not shapes, the way to override them is a little different than mentioned above.

### View Resolution paths

The `ThemeViewLocationExpanderProvider.cs` file defines the search paths used by the RazorViewEngine.

```
Views/{2}/{1}/{0}.cshtml
Views/{2}/Shared/{0}.cshtml
Views/Shared/{0}.cshtml
```

- 2 = area / module
- 1 = controller
- 0 = action

### Overriding Login view

For example, if you want to override the `OrchardCore.Users\Views\Account\Login.cshtml` view you would need to create a file in your theme and place it under `YourTheme\Views\OrchardCore.Users\Account\Login.cshtml`.  
For this particular file, you would also need to select the `Use site theme for login page`
option under the `Settings -> Security -> User Login` page in the admin.

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/8Hm9HeZMsDA" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/bwFH-C18rrA" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/pfPkPH7PN5w" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
