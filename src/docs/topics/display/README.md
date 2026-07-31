# Display

Orchard Core allows you to customize the look and feel of your application by integrating your own [theme](../../reference/glossary/README.md#theme) or by overriding the rendering of the [templates](../../reference/glossary/README.md#template) in Razor or Liquid syntax.

Rendering in Orchard Core is built around a few concepts:

- **Themes** package the layout, templates, and assets (CSS, JavaScript, images) that define the appearance of the site.
- **Shapes** are the dynamic objects that get rendered. Every piece of the page — the layout, a content item, a field, a menu — is a shape whose template can be replaced.
- **Templates** provide the markup for shapes. They can live as files in a theme (Liquid or Razor) or be edited from the admin with the Templates module. Naming a template after an *alternate* (e.g. `Content-BlogPost.Summary`) targets a specific content type or display type.
- **Placement** controls which shapes are displayed, in what order, and in which zone or tab.
- **Zones** are regions of the layout (header, footer, sidebars) in which widgets can be placed, per page, using layers.

## Customize your Display

- [Create a Theme](../../getting-started/theme.md)
- [Create a Theme from an HTML Template](theme-from-html-template.md)
- [Learn about Shapes](shapes.md)
- [Edit your Templates](../../reference/modules/Templates/README.md)
- [Learn the Liquid Syntax](../../reference/modules/Liquid/README.md)
- [Define the Placement](../../reference/modules/Placement/README.md)
- [Place Widgets with Layers](../../reference/modules/Layers/README.md)
- [Manage your Stylesheets and Scripts](../../reference/modules/Resources/README.md)
