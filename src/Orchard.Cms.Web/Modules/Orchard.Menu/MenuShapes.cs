using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Menu.Models;
using Orchard.Utility;

namespace Orchard.Menu
{
    public class MenuShapes : IShapeTableProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MenuShapes(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Menu")
                .OnDisplaying(displaying =>
                {
                    var menu = displaying.Shape;
                    string menuName = menu.MenuName;

                    menu.Classes.Add("menu-" + menuName.HtmlClassify());
                    menu.Classes.Add("menu");
                    menu.Metadata.Alternates.Add("Menu__" + EncodeAlternateElement(menuName));
                })
                .OnProcessing(processing =>
                {
                    dynamic menu = processing.Shape;
                    string contentItemId = menu.ContentItemId;
                    string identity = menu.Identity;

                    // Menu population is executed when processing the shape so that its value
                    // can be cached. IShapeDisplayEvents is called before the ShapeDescriptor
                    // events and thus this code can be cached.

                    var httpContext = _httpContextAccessor.HttpContext;
                    var shapeFactory = httpContext.RequestServices.GetService<IShapeFactory>();
                    var contentManager = httpContext.RequestServices.GetService<IContentManager>();

                    ContentItem menuContentItem = null;

                    if (contentItemId != null)
                    {
                        menuContentItem = contentManager.GetAsync(contentItemId).Result;
                    }
                    else if (!String.IsNullOrEmpty(identity))
                    {
                        var contentIdentityManager = httpContext.RequestServices.GetService<IContentIdentityManager>();
                        menuContentItem = contentIdentityManager.GetAsync(new ContentIdentity("identifier", identity)).Result;
                    }

                    menu.MenuName = contentManager.PopulateAspect<ContentItemMetadata>(menuContentItem).DisplayText;

                    var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                    if (menuItems == null)
                    {
                        return;
                    }

                    // The first level of menu item shapes is created.
                    // Each other level is created when the menu item is displayed.

                    foreach(var contentItem in menuItems)
                    {
                        dynamic shape = shapeFactory.Create("MenuItem", Arguments.From(new
                        {
                            ContentItem = contentItem,
                            Level = 0,
                            Menu = menu,
                        }));

                        menu.Items.Add(shape);
                    }

                });

            builder.Describe("MenuItem")
                .OnDisplaying(displaying =>
                {
                    var menuItem = displaying.Shape;
                    ContentItem menuContentItem = menuItem.ContentItem;
                    var menu = menuItem.Menu;
                    var menuName = menu.MenuName;
                    int level = menuItem.Level;

                    var httpContext = _httpContextAccessor.HttpContext;
                    var shapeFactory = httpContext.RequestServices.GetService<IShapeFactory>();

                    var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                    if (menuItems == null)
                    {
                        return;
                    }

                    foreach (var contentItem in menuItems)
                    {
                        dynamic shape = shapeFactory.Create("MenuItem", Arguments.From(new
                        {
                            ContentItem = contentItem,
                            Level = 0,
                            Menu = menu,
                            MenuName = menuName
                        }));

                        menuItem.Items.Add(shape);
                    }
                    var contentType = menuContentItem.ContentType;

                    menuItem.Metadata.Alternates.Add("MenuItem__level__" + level);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menuName) + "__level__" + level);

                    // MenuItem__[ContentType] e.g. MenuItem-HtmlMenuItem
                    // MenuItem__[ContentType]__level__[level] e.g. MenuItem-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(contentType));
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(contentType) + "__level__" + level);

                    // MenuItem__[MenuName]__[ContentType] e.g. MenuItem-Main-Menu-HtmlMenuItem
                    // MenuItem__[MenuName]__[ContentType] e.g. MenuItem-Main-Menu-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType));
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType) + "__level__" + level);
                });

            builder.Describe("MenuItemLink")
                .OnDisplaying(displaying =>
                {
                    var menuItem = displaying.Shape;
                    string menuName = menuItem.Menu.MenuName;
                    int level = menuItem.Level;

                    ContentItem menuContentItem = menuItem.ContentItem;
                    var contentType = menuContentItem.ContentType;

                    menuItem.Metadata.Alternates.Add("MenuItemLink__level__" + level);

                    // MenuItemLink__[ContentType] e.g. MenuItemLink-HtmlMenuItem
                    // MenuItemLink__[ContentType]__level__[level] e.g. MenuItemLink-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(contentType));
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(contentType) + "__level__" + level);

                    // MenuItemLink__[MenuName] e.g. MenuItemLink-Main-Menu
                    // MenuItemLink__[MenuName]__level__[level] e.g. MenuItemLink-Main-Menu-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__level__" + level);

                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem
                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType));
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType) + "__level__" + level);
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
