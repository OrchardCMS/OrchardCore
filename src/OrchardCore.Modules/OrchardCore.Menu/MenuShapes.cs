using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu
{
    public class MenuShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Menu")
                .OnDisplaying(displaying =>
                {
                    var menu = displaying.Shape;
                    string identifier = menu.ContentItemId ?? menu.Alias;

                    if (!String.IsNullOrEmpty(identifier))
                    {
                        menu.Classes.Add("menu");
                        menu.Metadata.Alternates.Add("Menu__" + EncodeAlternateElement(identifier));
                    }
                })
                .OnProcessing(async context =>
                {
                    var menu = context.Shape;
                    string identifier = menu.ContentItemId ?? menu.Alias;

                    if (String.IsNullOrEmpty(identifier))
                    {
                        return;
                    }

                    // Menu population is executed when processing the shape so that its value
                    // can be cached. IShapeDisplayEvents is called before the ShapeDescriptor
                    // events and thus this code can be cached.

                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                    var aliasManager = context.ServiceProvider.GetRequiredService<IContentAliasManager>();

                    string contentItemId = menu.Alias != null
                        ? await aliasManager.GetContentItemIdAsync(menu.Alias)
                        : menu.ContentItemId;

                    ContentItem menuContentItem = await contentManager.GetAsync(contentItemId);

                    if (menuContentItem == null)
                    {
                        return;
                    }

                    menu.MenuName = (await contentManager.PopulateAspectAsync<ContentItemMetadata>(menuContentItem)).DisplayText;

                    var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                    if (menuItems == null)
                    {
                        return;
                    }

                    // The first level of menu item shapes is created.
                    // Each other level is created when the menu item is displayed.

                    foreach (var contentItem in menuItems)
                    {
                        dynamic shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new
                        {
                            ContentItem = contentItem,
                            Level = 0,
                            Menu = menu,
                        }));

                        // Don't use Items.Add() or the collection won't be sorted
                        menu.Add(shape);
                    }

                });

            builder.Describe("MenuItem")
                .OnDisplaying(async context =>
                {
                    var menuItem = context.Shape;
                    ContentItem menuContentItem = menuItem.ContentItem;
                    var menu = menuItem.Menu;
                    int level = menuItem.Level;

                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();

                    var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                    if (menuItems != null)
                    {
                        foreach (var contentItem in menuItems)
                        {
                            dynamic shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new
                            {
                                ContentItem = contentItem,
                                Level = 0,
                                Menu = menu,
                            }));

                            // Don't use Items.Add() or the collection won't be sorted
                            menuItem.Add(shape);
                        }
                    }

                    var encodedContentType = EncodeAlternateElement(menuContentItem.ContentItem.ContentType);
                    var encodedContentItemId = EncodeAlternateElement(menuContentItem.ContentItem.ContentItemId);

                    menuItem.Metadata.Alternates.Add("MenuItem__level__" + level);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + encodedContentItemId);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + encodedContentItemId + "__level__" + level);

                    // MenuItem__[ContentType] e.g. MenuItem-HtmlMenuItem
                    // MenuItem__[ContentType]__level__[level] e.g. MenuItem-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItem__" + encodedContentType);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + encodedContentType + "__level__" + level);

                    // MenuItem__[MenuName]__[ContentType] e.g. MenuItem-Main-Menu-HtmlMenuItem
                    // MenuItem__[MenuName]__[ContentType] e.g. MenuItem-Main-Menu-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItem__" + encodedContentItemId + "__" + encodedContentType);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + encodedContentItemId + "__" + encodedContentType + "__level__" + level);
                });

            builder.Describe("MenuItemLink")
                .OnDisplaying(displaying =>
                {
                    var menuItem = displaying.Shape;
                    int level = menuItem.Level;

                    ContentItem menuContentItem = menuItem.ContentItem;

                    var encodedContentType = EncodeAlternateElement(menuContentItem.ContentItem.ContentType);
                    var encodedContentItemId = EncodeAlternateElement(menuContentItem.ContentItem.ContentItemId);

                    menuItem.Metadata.Alternates.Add("MenuItemLink__level__" + level);

                    // MenuItemLink__[ContentType] e.g. MenuItemLink-HtmlMenuItem
                    // MenuItemLink__[ContentType]__level__[level] e.g. MenuItemLink-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + encodedContentType);
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + encodedContentType + "__level__" + level);

                    // MenuItemLink__[MenuName] e.g. MenuItemLink-Main-Menu
                    // MenuItemLink__[MenuName]__level__[level] e.g. MenuItemLink-Main-Menu-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + encodedContentItemId);
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + encodedContentItemId + "__level__" + level);

                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem
                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + encodedContentItemId + "__" + encodedContentType);
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + encodedContentItemId + "__" + encodedContentType + "__level__" + level);
                });
        }

        /// <summary>
        /// Encodes dashed and dots so that they don't conflict in filenames
        /// </summary>
        /// <param name="alternateElement"></param>
        /// <returns></returns>
        private string EncodeAlternateElement(string alternateElement)
        {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }
    }
}
