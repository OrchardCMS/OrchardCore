using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Utilities;
using OrchardCore.Menu.Models;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Menu
{
    public class MenuShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Menu")
                .OnProcessing(async context =>
                {
                    var menu = context.Shape;

                    // Menu population is executed when processing the shape so that its value
                    // can be cached. IShapeDisplayEvents is called before the ShapeDescriptor
                    // events and thus this code can be cached.

                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                    var handleManager = context.ServiceProvider.GetRequiredService<IContentHandleManager>();

                    var contentItemId = menu.TryGetProperty("Alias", out object alias) && alias != null
                        ? await handleManager.GetContentItemIdAsync(alias.ToString())
                        : menu.TryGetProperty("ContentItemId", out object id)
                            ? id.ToString()
                            : null;

                    if (contentItemId == null)
                    {
                        return;
                    }

                    menu.Classes.Add("menu");

                    var menuContentItem = await contentManager.GetAsync(contentItemId);

                    if (menuContentItem == null)
                    {
                        return;
                    }

                    menu.Properties["ContentItem"] = menuContentItem;

                    menu.Properties["MenuName"] = menuContentItem.DisplayText;

                    var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                    if (menuItems == null)
                    {
                        return;
                    }

                    var differentiator = FormatName(menu.GetProperty<string>("MenuName"));

                    if (!String.IsNullOrEmpty(differentiator))
                    {
                        // Menu__[MenuName] e.g. Menu-MainMenu
                        menu.Metadata.Alternates.Add("Menu__" + differentiator);
                        menu.Metadata.Differentiator = differentiator;
                        menu.Classes.Add(("menu-" + differentiator).HtmlClassify());
                    }

                    // The first level of menu item shapes is created.
                    // Each other level is created when the menu item is displayed.

                    foreach (var contentItem in menuItems)
                    {
                        var shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new
                        {
                            ContentItem = contentItem,
                            Level = 0,
                            Menu = menu
                        }));

                        shape.Metadata.Differentiator = differentiator;

                        // Don't use Items.Add() or the collection won't be sorted
                        await ((Shape)menu).AddAsync(shape);
                    }
                });

            builder.Describe("MenuItem")
                .OnDisplaying(async context =>
                {
                    var menuItem = context.Shape;
                    var menuContentItem = menuItem.GetProperty<ContentItem>("ContentItem");
                    var menu = menuItem.GetProperty<IShape>("Menu");
                    var level = menuItem.GetProperty<int>("Level");
                    var differentiator = menuItem.Metadata.Differentiator;

                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();

                    var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                    if (menuItems != null)
                    {
                        foreach (var contentItem in menuItems)
                        {
                            var shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new
                            {
                                ContentItem = contentItem,
                                Level = level + 1,
                                Menu = menu
                            }));

                            shape.Metadata.Differentiator = differentiator;

                            // Don't use Items.Add() or the collection won't be sorted
                            await menuItem.AddAsync(shape);
                        }
                    }

                    var encodedContentType = menuContentItem.ContentItem.ContentType.EncodeAlternateElement();

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
                    var menuItem = displaying.Shape;
                    var level = menuItem.GetProperty<int>("Level");
                    var differentiator = menuItem.Metadata.Differentiator;

                    var menuContentItem = menuItem.GetProperty<ContentItem>("ContentItem");

                    var encodedContentType = menuContentItem.ContentItem.ContentType.EncodeAlternateElement();

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
