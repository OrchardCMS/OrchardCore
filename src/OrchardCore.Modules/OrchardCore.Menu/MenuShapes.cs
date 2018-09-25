using System;
using System.Text;
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
                .OnProcessing(async context =>
                {
                    dynamic menu = context.Shape;
                    string identifier = menu.ContentItemId ?? menu.Alias;

                    if (String.IsNullOrEmpty(identifier))
                    {
                        return;
                    }

                    menu.Classes.Add("menu");

                    // Menu population is executed when processing the shape so that its value
                    // can be cached. IShapeDisplayEvents is called before the ShapeDescriptor
                    // events and thus this code can be cached.

                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                    var aliasManager = context.ServiceProvider.GetRequiredService<IContentAliasManager>();

                    string contentItemId = menu.Alias != null
                        ? await aliasManager.GetContentItemIdAsync(menu.Alias)
                        : menu.ContentItemId;

                    if (contentItemId == null)
                    {
                        return;
                    }

                    var menuContentItem = await contentManager.GetAsync(contentItemId);

                    if (menuContentItem == null)
                    {
                        return;
                    }

                    menu.MenuName = menuContentItem.DisplayText;

                    var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                    if (menuItems == null)
                    {
                        return;
                    }

                    string differentiator = FormatName((string) menu.MenuName);

                    if (!String.IsNullOrEmpty(differentiator))
                    {
                        // Menu__[MenuName] e.g. Menu-MainMenu
                        menu.Metadata.Alternates.Add("Menu__" + differentiator);
                        menu.Differentiator = differentiator;
                    }

                    // The first level of menu item shapes is created.
                    // Each other level is created when the menu item is displayed.

                    foreach (var contentItem in menuItems)
                    {
                        var shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new
                        {
                            ContentItem = contentItem,
                            Level = 0,
                            Menu = menu,
                            Differentiator = differentiator
                        }));

                        // Don't use Items.Add() or the collection won't be sorted
                        menu.Add(shape);
                    }

                });

            builder.Describe("MenuItem")
                .OnDisplaying(async context =>
                {
                    dynamic menuItem = context.Shape;
                    ContentItem menuContentItem = menuItem.ContentItem;
                    var menu = menuItem.Menu;
                    int level = menuItem.Level;
                    string differentiator = menuItem.Differentiator;

                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();

                    var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                    if (menuItems != null)
                    {
                        foreach (var contentItem in menuItems)
                        {
                            var shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new
                            {
                                ContentItem = contentItem,
                                Level = 0,
                                Menu = menu,
                                Differentiator = differentiator
                            }));

                            // Don't use Items.Add() or the collection won't be sorted
                            menuItem.Add(shape);
                        }
                    }

                    var encodedContentType = EncodeAlternateElement(menuContentItem.ContentItem.ContentType);

                    // MenuItem__level__[level] e.g. MenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItem__level__" + level);

                    // MenuItem__[ContentType] e.g. MenuItem-HtmlMenuItem
                    // MenuItem__[ContentType]__level__[level] e.g. MenuItem-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItem__" + encodedContentType);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + encodedContentType + "__level__" + level);

                    if (!String.IsNullOrEmpty(differentiator))
                    {
                        // MenuItem__[MenuName] e.g. MenuItem-MainMenu
                        // MenuItem__[MenuName]__level__[level] e.g. MenuItem-MainMenu-level-2
                        menuItem.Metadata.Alternates.Add("MenuItem__" + differentiator);
                        menuItem.Metadata.Alternates.Add("MenuItem__" + differentiator + "__level__" + level);

                        // MenuItem__[MenuName]__[ContentType] e.g. MenuItem-MainMenu-HtmlMenuItem
                        // MenuItem__[MenuName]__[ContentType]__level__[level] e.g. MenuItem-MainMenu-HtmlMenuItem-level-2
                        menuItem.Metadata.Alternates.Add("MenuItem__" + differentiator + "__" + encodedContentType);
                        menuItem.Metadata.Alternates.Add("MenuItem__" + differentiator + "__" + encodedContentType + "__level__" + level);
                    }
                });

            builder.Describe("MenuItemLink")
                .OnDisplaying(displaying =>
                {
                    dynamic menuItem = displaying.Shape;
                    int level = menuItem.Level;
                    string differentiator = menuItem.Differentiator;

                    ContentItem menuContentItem = menuItem.ContentItem;

                    var encodedContentType = EncodeAlternateElement(menuContentItem.ContentItem.ContentType);

                    menuItem.Metadata.Alternates.Add("MenuItemLink__level__" + level);

                    // MenuItemLink__[ContentType] e.g. MenuItemLink-HtmlMenuItem
                    // MenuItemLink__[ContentType]__level__[level] e.g. MenuItemLink-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + encodedContentType);
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + encodedContentType + "__level__" + level);

                    if (!String.IsNullOrEmpty(differentiator))
                    {
                        // MenuItemLink__[MenuName] e.g. MenuItemLink-MainMenu
                        // MenuItemLink__[MenuName]__level__[level] e.g. MenuItemLink-MainMenu-level-2
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + differentiator);
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + differentiator + "__level__" + level);

                        // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-MainMenu-HtmlMenuItem
                        // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-MainMenu-HtmlMenuItem-level-2
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + differentiator + "__" + encodedContentType);
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + differentiator + "__" + encodedContentType + "__level__" + level);
                    }
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

        /// <summary>
        /// Converts "foo-ba r" to "FooBaR"
        /// </summary>
        private static string FormatName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            name = name.Trim();
            var nextIsUpper = true;
            var result = new StringBuilder(name.Length);
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];

                if (c == '-' || char.IsWhiteSpace(c))
                {
                    nextIsUpper = true;
                    continue;
                }

                if (nextIsUpper)
                {
                    result.Append(c.ToString().ToUpper());
                }
                else
                {
                    result.Append(c);
                }

                nextIsUpper = false;
            }

            return result.ToString();
        }
    }
}
