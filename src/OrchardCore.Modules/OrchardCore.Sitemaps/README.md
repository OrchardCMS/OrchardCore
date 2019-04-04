# Sitemaps (`OrchardCore.Sitemaps`)

The sitemaps module provides automatic generation of sitemaps.

## General Concepts

Sitemaps are configured by creating a Sitemap Set.
A Sitemap Set is a container for one, or many, Sitemap Nodes, and provides a base url that the Node(s) will be served under.

## How to create a Sitemap Set

* Ensure the Sitemaps module is enabled.

* Go to Configuration: Sitemaps.

* Create a new Sitemap Set, and add a Sitemap Node to it. The Sitemap Content Types Node may be all your site needs, and is a good place to get started.

* Set the base path for the Sitemap Set. If serving of the site root specify '/', or specify a further path, i.e. /blogs/

* As you create a Sitemap Node you will need to specify a path and filename, e.g. site.com/sitemap.xml.

* The sitemap can now be browsed to and will be served on that path.

## Sitemap Index Node

The Sitemap Index Node can contain multiple Sitemap Nodes. It allows you to create index sitemaps that contain metadata about other sitemaps.

This will produce xml conforming to the sitemapindex schema `<sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"/>`

It can only be created at the root of a Sitemap Set. You cannot contain indexes inside indexes.

## Sitemap Content Types Node

The Sitemap Content Types Node will provide a sitemap for your content items. The path will be based on the Sitemap Set's root path,
and the filename you choose. It must end in .xml.

This will produce xml that matches the urlset schema `<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"/>`

You can choose to serve all Content Types on this Node, or you can limit the Content Types on this Node. 

The latter configuration is generally used in combination with a Sitemap Index to limit the size of Sitemaps,
and make maintaining the Sitemap easier. 

Google limit the size of a sitemap to either 50,000 items, or 10MB whichever is reached first. 
If you need to limit the quantity of Content Items in a Sitemap uncheck Index All Content Types, select the Content Type to Index, 
uncheck Include All, and choose to Skip x number of Content Items and Take x number of Content Items. For the remaining Content Items,
create another Sitemap Content Types Node, and repeat choosing different values for Skip and Take as appropriate.
We strongly recommend if you need to do this to seperate the large quantity of Content Items into one Sitemap per large Content Type,
and serve the other Content Types from a different Sitemap Node.

You may also select the default Priority, and Change Frequency, either for all Content Types, or individual Content Types. 
To override this at an individual Content Item level you will need to attach the SitemapPart.

Note: If the Homepage Content Item is provided in the selection of Content Types it will use the site root as it's url,
rather than any other route that is assigned to it from the AutoroutePart. This is so that Google does not attempt to index the page,
on it's 'other' url.

//TODO ad tenant prefix to base root

## SitemapPart

Add the SitemapPart to a content item to provide sitemap configuration at a Content Item level.
Settings here will override any Sitemap Node configuration.

Check to override the Sitemap Set configuration.
You can choose to exclude just this Content Item from any Sitemap Node, or alter the Priority, or Change Frequency.

//TODO remove part settings, they're unecesary'


## Extending the Content Types Sitemap Node

## Developing Custom Sitemap Nodes

Any module can add it's own custom sitemap nodes so that they can be used by users to build sitemaps.

Commonly the steps that you follow in order to do that are:

* Add a class that inherits from `SitemapNode`. On this class add the specific properties that you want for your node type. This is the info that will go into the database.

* Add a Driver to handle the display and edit of your sitemap node on the Sitemap. This won't handle the actual rendering of the sitemap node. Drivers are only about the views required to create and edit the sitemap node.

* Optionally, you could implement a ViewModel to move info between the edit views and the driver.

* Create the views required to create and edit the sitemap nodes based on your node type.

* Add a class that inherits from SitemapNodeBuilderBase<TCustomSitemapNode>. 

* Implement a BuildNodeAsync() method to generate the XDocument that will be served for your sitemap.

* Optionally, implement the ProvideNodeLastModifiedDateAsync() method to provide a last modified date for any Sitemap Index Node that contains this Custom Sitemap Node.


By convention you should store all these non-view classes on a "SitemapNodes" folder. This is optional.

By convention you have to store the views on a "Items" folder inside the "Views" folder. This is required.

Don't forget to register the corresponding classes on the Startup class.

## Extending the Content Types Sitemap Node

The Content Types Sitemap Node is also extendable, for example, you could use the basic logic from it to extend and implement an Image Sitemap.

Most of the steps are similar to creating a Custom Sitemap Node with the following differences

* Extend your Node class from `ContentTypesSitemapNode`

* Extend the `UrlsetSitemapNodeBuilderBase<TYourSitemapNode>` class

* Override methods such as `GetNamespace()` to add to the xml namespace

* Override `GetContentItemsToBuildAsync()` to change how you are querying for Content Items

* Override `BuildUrlAsync()` to include extra metadata from the ContentItem in the urlset


## CREDITS

### IDeliverable.Seo

<https://github.com/IDeliverable/IDeliverable.Seo>  

Copyright (c) IDeliverable, Ltd. 

BSD-3

### jQuery UI Nested Sortable
 
v 2.1a / 2016-02-04
<https://github.com/ilikenwf/nestedSortable>

Depends on:
jquery.ui.sortable.js 1.10+

Copyright (c) 2010-2016 Manuele J Sarfatti and contributors
Licensed under the MIT License
<http://www.opensource.org/licenses/mit-license.php>