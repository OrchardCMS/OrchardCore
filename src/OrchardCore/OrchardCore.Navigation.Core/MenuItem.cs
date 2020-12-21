using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Navigation
{
    /// <summary>
    /// Represents a menu item described by an <see cref="INavigationProvider"/> implementation.
    /// A menu item can describe child menu items.
    /// </summary>
    public class MenuItem
    {
        public MenuItem()
        {
            Permissions = new List<Permission>();
            Classes = new List<string>();
            Items = new List<MenuItem>();
            LinkToFirstChild = true;
        }

        /// <summary>
        /// The text to display with the menu item.
        /// </summary>
        public LocalizedString Text { get; set; }

        /// <summary>
        /// The html id of the menu item.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The final url that the menu item will link to. This property is computed
        /// based on <see cref="Url"/> or <see cref="RouteValues"/>.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// The optional url the menu item should link to.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The relative position of the menu item among its siblings. e.g., 10, 0, "after".
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// The priority of the menu item in case there are other menu items that could be marked as selected for the request.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Whether the menu item should link to the same url as its first child.
        /// </summary>
        public bool LinkToFirstChild { get; set; }

        /// <summary>
        /// <c>True</c> if the menu item is local to the page, like a tab.
        /// </summary>
        public bool LocalNav { get; set; }

        /// <summary>
        /// The culture for which this menu item is used.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// The resource the permission is protecting.
        /// </summary>
        [JsonIgnore]
        public object Resource { get; set; }

        /// <summary>
        /// The child menu items.
        /// </summary>
        public List<MenuItem> Items { get; set; }

        /// <summary>
        /// The optional route values for this menu item.
        /// </summary>
        [JsonIgnore]
        public RouteValueDictionary RouteValues { get; set; }

        /// <summary>
        /// The list of <see cref="Permission"/> objects the user must have in order to see this menu item.
        /// </summary>
        [JsonIgnore]
        public List<Permission> Permissions { get; }

        /// <summary>
        /// The css classes to render with the menu item.
        /// </summary>
        public List<string> Classes { get; }
    }
}
