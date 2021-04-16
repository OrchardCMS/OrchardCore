# Glossary

List of terms and concepts that you can find in Orchard Core.

They are grouped by roles: User, Theme designer, Administrator.

## Authenticated users

### Content Item

A single document containing some content of a specific content type, that can be versioned and localized. Examples of content items are pages, blog posts and products. They are often associated with a unique URL (address) on the site.

### Content Item Version

A single document that represent a specific version of a content item. These can be draft, published, or pasts versions

### Content Type

Define the list of Content Parts and Content Fields a content item can be made of. An analogy is to compare them to classes, whose instances are the content items.

### Content Part

Content parts are atoms of content that are enough to build a specific coherent behavior and that can be reused across content types.
See [Content Parts](../reference/modules/ContentParts/README.md)

### Content Field

A content field extends a content type with a named piece of data. There can be multiple fields of the same kind attached to a content type or a content part. For instance __Description__ could be a field defined on a __Product__ content type.
See [Content Fields](../reference/modules/ContentFields/README.md)

### Display Type

A way to specify in which context a content element is rendered (ex: Details, Summary, SummaryAdmin). A template can be defined for each display type.

### Field Editor

A field can have different Editors (Ex: The value of a Numeric Field can be set by an input or a slider).

### Autoroute

A part that dynamically creates and registers a url to access a content item. It can use a Liquid pattern to be automatically generated.
See [Autoroute](../reference/modules/Autoroute/README.md)

### Bag

A collection of content items of a certain type in a parent content item. The content items are aggregated in the bag.

### List

A list of content items to a parent container (Ex: A blog contains a list of blog posts). The content items are referenced.
See [Lists](../reference/modules/Lists/README.md)

### Taxonomy

A hierarchy of content items that are used as references for other content items. Also called managed vocabularies. An example is a list of colors which can then be attached to products. Also often used for categories of articles for instance.
See [Taxonomies](../reference/modules/Taxonomies/README.md)

### Admin menu

A hierarchy of menu items that are displayed in the admin section of the site.
See [Admin menu](../reference/modules/AdminMenu/README.md)

### Alias

A part that allows you to specify an alias. A way to identify an item with a key that you can call to retrieve it, instead of an Id.  
See [Alias](../reference/modules/Alias/README.md)

### Content Preview

Allows to Preview and Live Edit a content.  
See [Content Preview](../reference/modules/ContentPreview/README.md)

### Indexing

Define the way the content will be indexed in order to search it from a query.  
See [Indexing](../reference/modules/Indexing/README.md)

### Query

Parameterized Lucene or Sql query defined in admin.  
See [Queries](../reference/modules/Queries/README.md)

### Tenant

An independent subsite with its own url.  
One instance can have multiple tenants.  
They can only be managed in the Default one.  
See [Tenants](../reference/modules/Tenants/README.md)

## Theme Designer

### Theme

A module that contains assets (Images, Styles, Scripts) and views used to customize the display.  
It can also contain a recipe to initialize some content types and content items.

### Liquid

A syntax that you can use in the views instead of Razor or in the Templates.  
See [Liquid](../reference/modules/Liquid/README.md)

### Alternate

An override of content type or part or field using a file in a Theme.  
See [Alternates](../reference/modules/Templates//README.md#shape-differentiators)

### Placement

A mapping file to set the order of appearance or hide contents for a specific content type or Part/Field name or display type in a Theme.  
See [Placement](../reference/core/Placement/README.md)

### Assets

In the Admin, this is the Media library. In a Theme, these are the folders in wwwroot.

### Resource

Style or Script libraries that are registered with a specific version and with potentially minified file and CDN urls.  
See [Resources](../reference/modules/Resources/README.md)

### Shape

 [GitHub Discussion](https://github.com/OrchardCMS/OrchardCore/issues/4121#issuecomment-539608731)

### Template

A Liquid override of a Shape or a Display type in admin.  
See [Templates](../reference/modules/Templates/README.md)

### Zone

A section in the Layout (ex: Footer) in which you can render items.

### Layer

A display Rule in which you specify a condition to be rendered (ex: isHomepage()).  
See [Layers](../reference/modules/Layers/README.md)

### Stereotype

By default Content Items have no stereotype, however certain modules will use a defined Stereotype to determine which content types can be used by them.  
Examples of this include the Menu Stereotype, and the Widget Stereotype.

### Widget

A content displayed in a specific zone and layer.  
It has the `Widget` stereotype in its content definition.

### Flow

Page Layout in which you can add widgets.  
See [Flow](../reference/modules/Flow/README.md)

### Shortcode

A Shortcode is a small piece of code wrapped into [brackets] that can add some behavior to content editors, like embedding media files.  
See [Shortcodes](../reference/modules/Shortcodes/README.md)

## Administrator

### Recipe

A json file used to execute different import and configuration steps.

### Setup Recipe

Import steps like Set theme, Define types, Import data, ... executed during setup.

### Permission

Allow Users in a Role to have access to a specific action.

### Deployment Step

An export of a specific set of information (Configuration or Data).

### Deployment Plan

A batch Export that executes multiple deployment steps.
