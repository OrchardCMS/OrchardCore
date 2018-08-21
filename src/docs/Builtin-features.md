# Built-in modules and features

There are lots of features available in Orchard Core out of the box.  
This topic gives a brief description of each of Orchard Core's first-party features.

## OrchardCore.Admin

Implements the administration dashboard as an extensible shell.

## OrchardCore.Navigation

The application ships with a hierarchical and extensible navigation menu.
More than one named menu can be built using this core module. Menu items can be provided
by any number of providers. The module ships with custom items that can point to any URL,
and content menu items that point to a specific content item.
This core module also provides the administration menu.

## OrchardCore.Title

This simple module introduces the Title part that is used by most content types.

## OrchardCore.Html

Provides the HtmlBody part with different text editors to manage rich content.

## OrchardCore.Alias

The Alias module sets up the infrastructure to map friendly URLs to content items
and custom routes. It is the foundation on which Autoroute is built.

## OrchardCore.Autoroute

This very powerful feature makes it possible for content type creators to specify a
liquid-based URL blueprint.

## OrchardCore.Contents

Provides a UI to access to the content items.

### Common

The Common part captures the creation, modification and publication dates, as well as the
owner of the content item.

## Core shapes

Shapes in Orchard are the basic units of UI from which all HTML is built. New shapes can of course
be added by modules dynamically, but this module provides some basic and standard shapes.

Core shapes are defined in code in CoreShapes.cs. Other shapes are defined in Views as cshtml files.
Any shape, core or otherwise, can be overridden by templates in a theme.

* ActionLink: uses route values to construct a HTML link.
* DisplayTemplate, EditorTemplate: internally used to build the display and editor views of content items.
* Layout: this is the outermost shape, that together with its wrapper, Document, defines the basic HTML structure to be rendered. It also defines top-level zones.
* List: a standard shape to render lists of shapes.
* Menu, MenuItem, LocalMenu, LocalMenuItem and MenuItemLink: the shapes that navigation renders.
* Pager and associated shapes and alternates: the shapes used to render pagination.
* Partial: a shape that can be used to render a template as a partial view, using the specified model. Creating a dynamic shape is often a preferable technique.
* Resource, HeadScripts, FootScripts, Metas, HeadLinks, StyleSheetLinks, Style: shapes used to render resources such as script tags or stylesheets.
* Zone: a special shape that is made to contain other shapes, typically but not always limited to widgets.

## OrchardCore.ContentFields

Provides Input, Boolean, DateTime, Numeric, Link, Enumeration, and Media Picker fields
that can be used in custom content types.

## OrchardCore.ContentPreview

The content Preview module enables live content edition and content preview.

## OrchardCore.ContentTypes

Enable this module to enable the creation and modification of content types from the admin UI.

## OrchardCore.CustomSettings

The custom settings modules enables content types to become custom site settings.

## OrchardCore.Deployment

The definition for content types, as well as the content itself, can be exported from one
instance, and imported into another using this module.

## OrchardCore.Deployment.Remote

Provides the ability to export and import to and from a remote server.

## OrchardCore.DynamicCache

Allows you to cache sections of markup thanks to tag helper or liquid syntax.

## OrchardCore.DisplayManagement

Allows to add a `placement.json` file providing custom placement logic.

## OrchardCore.Email

This module implements an email messaging channel that can be used for example to send
email notifications

## OrchardCore.Features

This is the module that provides the admin UI to enable and disable features.

## OrchardCore.Feeds

The Feeds module provides the infrastructure that modules can use to expose RSS,
Atom or other types of feeds.

## OrchardCore.Flow

Provides a Flow part allowing users to edit their content based on Widgets.

## OrchardCore.Forms

Provides widgets and activities to implement custom forms.

## OrchardCore.Https

This module will ensure Https is used when accessing specific parts of the website like the dashboard, authentication pages or custom pages.

## OrchardCore.Indexing, OrchardCore.Search and OrchardCore.Lucene

Those three modules constitute the default full-text search infrastructure for OrchardCore.
The indexing module populates the index from content items. The Lucene module provides
the specific index implementation that indexing populates and that search queries.
The search index queries the index and formats results.

## OrchardCore.Liquid

Provides a system for performing string replacements with common site values. For example,
the Autoroute feature makes it possible to define URL patterns for content items of a given
type.

## OrchardCore.Lists

This module provides a simple implementation for lists of content items where a content item can belong to only one list.

## OrchardCore.Localization

The localization module enables the localization of content items. This module provides a part that can be added to a content type to make it localizable. The items of the modified types can have several versions that differ by culture.

## OrchardCore.Markdown

[Markdown][2] is a human-readable text format used to describe rich text without the
complexity of HTML. Some people prefer to write in Markdown rather than in a WYSYWYG
text editor such as the editor that comes with OrchardCore.Html.

## OrchardCore.Media

Provides enhanced Media management tools.

## OrchardCore.Media.Azure

Provides integration of Microsoft Azure Media Services functionality.

## OrchardCore.Menu

Provides menu management features as shapes, tag helper and UI to manage menu items.

## OrchardCore.Queries

This tremendously useful module enables the creation of queries over
the contents of the site that you can then call using the Liquid syntax.

## OrchardCore.Queries.Sql

Introduces a way to create custom Queries in pure SQL.

## OrchardCore.Recipes

Recipes are json files that describe a set of operations on the contents and configuration
of the site. Recipes are used at setup to describe predefined initial configurations
(Orchard Core comes with sass, blog and agency recipes). They can also be included with
modules to specify additional operations that get executed after installation.

## OrchardCore.Resources

Provides a variety of third-party client-side libraries (stylesheets and scripts) used by other modules.

## OrchardCore.ResponseCache

Provides Response Caching functionality.

## OrchardCore.Roles

Provides the ability to assign roles to users. It's also providing a set of default roles for which other modules can define default permissions.

## OrchardCore.Scripting.JavaScript

In order to enable simple programmability of the application without requiring the
development of a whole module, certain key areas of Orchard Core expose extensibility through
scripting. For example, widget layer visibility is defined by rules that are written
as simple script expressions.

## OrchardCore.Settings

The settings module creates site settings that other modules can contribute to.

## OrchardCore.Setup

This module is always disabled except before the application has been setup. It is responsible
for implementing the setup mechanism.

## OrchardCore.Tenants

Hosting multiple Orchard sites on separate applications means duplicating everything
for each site. The multi-tenancy module enables the hosting of multiple Orchard sites
within a single IIS application, thus saving a lot of resources, and reducing maintenance
costs. Each site's data is strictly segregated from the others through a table prefix
or complete database separation.

## OrchardCore.Themes

This module provides the infrastructure for easy customization of the look and feel of the site
through the definition of themes, which are a set of scripts, stylesheets and template overrides.

## OrchardCore.Templates

Provides a way to write custom shape templates with Liquid syntax from the admin.

## OrchardCore.Users

This is the module that implements the default user management in Orchard Core.

## OrchardCore.Widgets

Widgets are reusable pieces of UI that can be positioned on any page of the site. Which widgets
get displayed on what pages is determined by layer rules.

## OrchardCore.Workflows

OrchardCore.Workflows module provides tools to create custom workflows.

## OrchardCore.XmlRpc

The APIs necessary to create content like blog posts from applications such as
Windows Live Writer are implemented in this core module.
