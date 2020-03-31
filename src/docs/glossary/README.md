# Glossary

List of terms and concepts that you can find in Orchard Core.

They are grouped by roles: User, Theme designer, Administrator.

If you think that there is a missing definition, or that a definition needs to be revised, updated, please create an issue or make a Pull Request.  
Note that each definition should consist of:
1.	A clear statement of what the item being defined is in an OrchardCore context
2.	A clear statement of what the item does in an OrchardCore context
3.	Clear examples of how the item is used in an OrchardCore context 

## Authenticated users

### Content Item

The consumable version of a piece of web content that can be published for display via a web browser. In the OrchardCore Admin GUI, it is created from a content type. It can be versioned, localized, and can have its own unique URL (depending on its Content Parts). The display of a Content Item can be restricted by a user’s assigned permissions via a role or otherwise. A few examples are a web page, a blog post, or a product listing.

### Content Item Version

A single document that represent a specific version of a content item. These can be draft, published, or pasts versions

### Content Definition Objects

Objects in OrchardCore that determine the behavior, characteristics and type of content a Content Item can be made of. The Content Definition Objects are Content Part and a Content Type.

### Content Type

A blueprint from which a content item is created. It defines what features and characteristics the content item can have. A Content Type is made up of Content Parts which help to define is features and behavior.

### Content Part


A Content Part is smallest content building component in OrchardCore. It defines a specific feature or a specific behavior of a content type. A single Content Part can be used across multiple Content Types.
See [Content Parts](../reference/modules/ContentParts)

### Content Field

A content field extends a content type with a named piece of data. There can be multiple fields of the same kind attached to a content type or a content part. For instance __Description__ could be a field defined on a __Product__ content type.
See [Content Fields](../reference/modules/ContentFields)

### Display Type

A way to specify in which context a content element is rendered (ex: Details, Summary, SummaryAdmin). A template can be defined for each display type.

### Field Editor

A field can have different Editors (Ex: The value of a Numeric Field can be set by an input or a slider).

### Autoroute

A Content Part that can be added to a Content Type to provide a dynamic URL feature. This allows any Content Item built from the resulting Content Type to be directly accessible via a web address. An Autoroute can use a Liquid pattern to automatically generate the desired URL.
See [Autoroute](../reference/modules/Autoroute)

### Bag

A collection of content items of a certain type in a parent content item. The content items are aggregated in the bag.

### List

A list of content items to a parent container (Ex: A blog contains a list of blog posts). The content items are referenced.
See [List](../reference/modules/List)

### Taxonomy

A hierarchy of content items that are used as references for other content items. Also called managed vocabularies. An example is a list of colors which can then be attached to products. Also often used for categories of articles for instance.
See [Taxonomies](../reference/modules/Taxonomies)

### Admin menu

A hierarchy of menu items that are displayed in the admin section of the site.
See [Admin menu](../reference/modules/AdminMenu)

### Alias

A part that allows you to specify an alias. A way to identify an item with a key that you can call to retrieve it, instead of an Id.  
See [Alias](../reference/modules/Alias)

### Content Preview

Allows to Preview and Live Edit a content.  
See [Content Preview](../reference/modules/ContentPreview)

### Indexing

Define the way the content will be indexed in order to search it from a query.  
See [Indexing](../reference/modules/Indexing)

### Query

Parameterized Lucene or Sql query defined in admin.  
See [Queries](../reference/modules/Queries)

### Tenant

An independent subsite with its own url.  
One instance can have multiple tenants.  
They can only be managed in the Default one.  
See [Tenants](../reference/modules/Tenants)

## Theme Designer

### Theme

A module that contains assets (Images, Styles, Scripts) and views used to customize the display.  
It can also contain a recipe to initialize some content types and content items.

### Liquid

A syntax that you can use in the views instead of Razor or in the Templates.  
See [Liquid](../reference/modules/Liquid)

### Alternate

An override of content type or part or field using a file in a Theme.  
See [Alternates](../reference/modules/Templates/#shape-differentiators)

### Placement

A mapping file to set the order of appearance or hide contents for a specific content type or Part/Field name or display type in a Theme.  
See [Placement](../reference/modules/Placement)

### Assets

In the Admin, this is the Media library. In a Theme, these are the folders in wwwroot.

### Resource

Style or Script libraries that are registered with a specific version and with potentially minified file and CDN urls.  
See [Resources](../reference/modules/Resources)

### Shape

An OrchardCore object used to dynamically render HTML content. A Shape defines the type of content to be rendered and it receives its instructions on how to display the content it will render from a template. This makes the shape able to dynamically adapt the way it displays its content based the templates it receives from any OrchardCore theme.  
See [GitHub Discussion](https://github.com/OrchardCMS/OrchardCore/issues/4121#issuecomment-539608731)

### Template

A Liquid override of a Shape or a Display type in admin.  
See [Templates](../reference/modules/Templates)

### Zone

A section in the Layout (ex: Footer) in which you can render items.

### Layer

A display rule that defines the condition when a widget will be displayed. (ex: isHomepage()).  
See [Layers](../reference/modules/Layers)

### Stereotype

By default Content Items have no stereotype, however certain modules will use a defined StereoType to determine which content types can be used by them.  
Examples of this include the Menu Stereotype, and the Widget Stereotype.

### Widget

A widget is used to designate a prebuilt Content Item that can be used in a layer. This allows the content item to be displayed on multiple pages at once. Examples are a Menu, Footer, or Header.  
see [Widgets](../reference/modules/Widgets/)

### Flow

Page Layout in which you can add widgets.  
See [Flow](../reference/modules/Flow)

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
