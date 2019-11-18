# Glossary

List of terms and concepts that you can find in Orchard Core and a small explanation.

They are grouped by roles: Power user, Theme designer, Administrator.

## Power user

### Content Part

Extends a content type with a complex piece.  
There can be only one part of the same kind by content type.  
See [Content Parts](../reference/modules/ContentParts)

### Display type

A way to specify in which context a content is rendered (ex: Details, Summary, SummaryAdmin).  
A content can be displayed differently depending on which Display type is rendered.

### Content Field

Extend a content type with a simple piece. There can be multiple fields of the same kind attached to a content type.  
See [Content Fields](../reference/modules/ContentFields)

### Field Editor

A field can have different Editors (Ex: The value of a Numeric Field can be set by an input or a slider).

### Autoroute

Custom Permalink used for Routing.  
It can use a Liquid pattern to be automatically generated.  
See [Autoroute](../reference/modules/Autoroute)

### Bag

Collection of content items of a certain type in a parent content item.

### List

To attach content items to a parent Container (Ex: A blog contains a list of blog posts).  
See [List](../reference/modules/List)

### Taxonomy

Category that contains specific terms.  
See [Taxonomies](../reference/modules/Taxonomies)

### Admin menu

Custom link added in the left admin menu.  
See [Admin menu](../reference/modules/AdminMenu)

### Alias

A way to identify an item with a key that you can call to retrieve it, instead of an Id.  
See [Alias](../reference/modules/Alias)

### Content Preview

Allows to Preview and Live Edit a content.  
See [Content Preview](../reference/modules/ContentPreview)

### Indexing

Define the way the content will be indexed in order to be search it from a query.  
See [Indexing](../reference/modules/Indexing)

### Query

Parameterized Lucene or Sql query defined in admin.  
See [Queries](../reference/modules/Queries)

### Tenant

Independent subsite with its own url.  
One instance can have multiple tenants.  
They can only be managed in the Default one.  
See [Tenants](../reference/modules/Tenants)

## Theme Designer

### Theme

Module that contains assets (Images, Styles, Scripts) and views used to customize the display.  
It can also contain a recipe to initialize some content types and content items.

### Liquid

Code Syntax that you can use in the views instead of Razor or in the Templates.  
See [Liquid](../reference/modules/Liquid)

### Alternate

Override of content type or part or field using a file in a Theme.  
See [Alternates](../reference/modules/Templates/#shape-differentiators)

### Placement

Mapping file to set the order of appearance or hide contents for a specific content type or Part/Field name or display type in a Theme.  
See [Placement](../reference/modules/Placement)

### Assets

In the Admin, this is the Media library. In a Theme, these are the folders in wwwroot.

### Resource

Style or Script libraries that are registered with a specific version and with potentially minified file and CDN urls.  
See [Resources](../reference/modules/Resources)

### Shape

 [GitHub Discussion](https://github.com/OrchardCMS/OrchardCore/issues/4121#issuecomment-539608731)

### Template

Liquid Override of a Shape or a Display type in admin.  
See [Templates](../reference/modules/Templates)

### Zone

Section in the Layout (ex: Footer) in which you can render items.

### Layer

Display Rule in which you specify a condition to be rendered (ex: isHomepage()).  
See [Layers](../reference/modules/Layers)

### Widget

Display Content in a specific zone and layer.

### Flow

Page Layout in which you can add widgets.  
See [Flow](../reference/modules/Flow)

## Administrator

### Recipe

Json file used to import Wizard with different steps to execute.

### Setup Recipe

Import steps like Set theme, Define types, Import data, ... executed during setup.

### Permission

Allow a Users in a Role to do a have access to a specific action.

### Deployment Step

Export of a specific set of information (Configuration or Data).

### Deployment Plan

Batch Export that executes multiple deployment steps.
