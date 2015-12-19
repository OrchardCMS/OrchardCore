using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.UI;
using Orchard.Utility;
using Microsoft.AspNet.Mvc.Rendering;

namespace Orchard.Core.Navigation
{
    public class MenuShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {

            builder.Describe("Menu")
                .OnDisplaying(displaying => {
                    var menu = displaying.Shape;
                    string menuName = menu.Name;
                    menu.Classes.Add("menu-" + menuName.HtmlClassify());
                    menu.Classes.Add("menu");
                    menu.Metadata.Alternates.Add("Menu__" + EncodeAlternateElement(menuName));
                });

            builder.Describe("MenuItem")
                .OnDisplaying(displaying => {
                    var menuItem = displaying.Shape;
                    var menu = menuItem.Menu;
                    var menuName = menu.Name;
                    int level = menuItem.Level;

                    menuItem.Metadata.Alternates.Add("MenuItem__level__" + level);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menuName) + "__level__" + level);
                });

            builder.Describe("MenuItemLink")
                .OnDisplaying(displaying => {
                    var menuItem = displaying.Shape;
                    string menuName = menuItem.Menu.Name;
                    string contentType = null;
                    int level = menuItem.Level;

                    menuItem.Metadata.Alternates.Add("MenuItemLink__level__" + level);

                    // MenuItemLink__[ContentType] e.g. MenuItemLink-HtmlMenuItem
                    // MenuItemLink__[ContentType]__level__[level] e.g. MenuItemLink-HtmlMenuItem-level-2
                    if (contentType != null)
                    {
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(contentType));
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(contentType) + "__level__" + level);
                    }

                    // MenuItemLink__[MenuName] e.g. MenuItemLink-Main-Menu
                    // MenuItemLink__[MenuName]__level__[level] e.g. MenuItemLink-Main-Menu-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__level__" + level);

                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem
                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem-level-2
                    if (contentType != null)
                    {
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType));
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType) + "__level__" + level);
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
    }
}