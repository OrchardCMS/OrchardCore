using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;

namespace Orchard.Environment.Navigation
{
    public class NavigationManager : INavigationManager
    {
        private static string[] Schemes = new[] { "http", "https", "tel", "mailto" };

        private readonly IEnumerable<INavigationProvider> _navigationProviders;
        private readonly ILogger _logger;
        protected readonly ShellSettings _shellSettings;
        private readonly IUrlHelperFactory _urlHelperFactory;
        //private readonly IAuthorizationService _authorizationService;

        private IUrlHelper _urlHelper;

        public NavigationManager(
            IEnumerable<INavigationProvider> navigationProviders,
            ILogger<NavigationManager> logger,
            ShellSettings shellSettings,
            IUrlHelperFactory urlHelperFactory
            //IAuthorizationService authorizationService ,
            )
        {
            _navigationProviders = navigationProviders;
            _logger = logger;
            _shellSettings = shellSettings;
            _urlHelperFactory = urlHelperFactory;
            //_authorizationService = authorizationService;
        }

        public IEnumerable<MenuItem> BuildMenu(string name, ActionContext actionContext)
        {
            var builder = new NavigationBuilder();

            // Processes all navigation builders to create a flat list of menu items.
            // If a navigation builder fails, it is ignored.
            foreach (var navigationProvider in _navigationProviders)
            {
                try
                {
                    navigationProvider.BuildNavigation(name, builder);
                }
                catch (Exception e)
                {
                    _logger.LogError($"An exception occured while building the menu: {name}", e);
                }
            }

            var menuItems = builder.Build();

            // Merge all menu hierarchies into a single one
            Merge(menuItems);

            // Remove unauthorized menu items
            menuItems = Reduce(menuItems, null);

            // Compute Url and RouteValues properties to Href
            menuItems = ComputeHref(menuItems, actionContext);

            return menuItems;
        }

        /// <summary>
        /// Mutates a list of <see cref="MenuItem"/> into a hierarchy
        /// </summary>
        private static void Merge(List<MenuItem> items)
        {
            // Use two cursors to find all similar captions. If the same caption is represented
            // by multiple menu item, try to merge it recursively.
            for (var i = 0; i < items.Count; i++)
            {
                var source = items[i];
                var merged = false;
                for (var j = items.Count - 1; j > i ; j--)
                {
                    var cursor = items[j];

                    // A match is found, add all its items to the source
                    if(String.Equals(cursor.Text.Name, source.Text.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        merged = true;
                        foreach (var child in cursor.Items)
                        {
                            source.Items.Add(child);
                        }

                        items.RemoveAt(j);

                        // If the item to merge is more authoritative then use its values
                        if (cursor.Position != null && source.Position == null)
                        {
                            source.Culture = cursor.Culture;
                            source.Href = cursor.Href;
                            source.Id = cursor.Id;
                            source.LinkToFirstChild = cursor.LinkToFirstChild;
                            source.LocalNav = cursor.LocalNav;
                            source.Position = cursor.Position;
                            source.Resource = cursor.Resource;
                            source.RouteValues = cursor.RouteValues;
                            source.Text = cursor.Text;
                            source.Url = cursor.Url;

                            source.Permissions.Clear();
                            source.Permissions.AddRange(cursor.Permissions);

                            source.Classes.Clear();
                            source.Classes.AddRange(cursor.Classes);
                        }
                    }
                }

                // If some items have been merged, apply recursively
                if (merged)
                {
                    Merge(source.Items);
                }
            }
        }

        /// <summary>
        /// Computes the <see cref="MenuItem.Href"/> properties based on <see cref="MenuItem.Url"/>
        /// and <see cref="MenuItem.RouteValues"/> values.
        /// </summary>
        private List<MenuItem> ComputeHref(List<MenuItem> menuItems, ActionContext actionContext)
        {
            foreach (var menuItem in menuItems)
            {
                menuItem.Href = GetUrl(menuItem.Url, menuItem.RouteValues, actionContext);
                menuItem.Items = ComputeHref(menuItem.Items, actionContext);
            }

            return menuItems;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="menuItemUrl"></param>
        /// <param name="routeValueDictionary"></param>
        /// <returns></returns>
        private string GetUrl(string menuItemUrl, RouteValueDictionary routeValueDictionary, ActionContext actionContext)
        {
            string url;
            if (routeValueDictionary == null || routeValueDictionary.Count == 0)
            {
                if (!String.IsNullOrEmpty(menuItemUrl))
                {
                    return "#";
                }
                else
                {
                    url = menuItemUrl;
                }
            }
            else
            {
                if (_urlHelper == null)
                {
                    _urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                }

                url = _urlHelper.RouteUrl(new UrlRouteContext { Values = routeValueDictionary });
            }

            if (!string.IsNullOrEmpty(url) &&
                actionContext?.HttpContext != null &&
                !(url.StartsWith("/") ||
                Schemes.Any(scheme => url.StartsWith(scheme + ":"))))
            {
                if (url.StartsWith("~/"))
                {
                    if (!String.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                    {
                        url = _shellSettings.RequestUrlPrefix + "/" + url.Substring(2);
                    }
                    else
                    {
                        url = url.Substring(2);
                    }
                }

                if (!url.StartsWith("#"))
                {
                    var appPath = actionContext.HttpContext.Request.PathBase.ToString();
                    if (appPath == "/")
                        appPath = "";
                    url = appPath + "/" + url;
                }
            }
            return url;
        }

        /// <summary>
        /// Updates the items by checking for permissions
        /// </summary>
        private List<MenuItem> Reduce(IEnumerable<MenuItem> items, ClaimsPrincipal user)
        {
            var filtered = new List<MenuItem>();

            foreach (var item in items)
            {
                // TODO: Attach actual user and remove this clause
                if(user == null)
                {
                    filtered.Add(item);
                }
                else if(!item.Permissions.Any())
                {
                    filtered.Add(item);
                }
                else
                {
                    //var policy = new AuthorizationPolicyBuilder()
                    //    .RequireClaim(Permission.ClaimType, item.Permissions.Select(x => x.Name))
                    //    .Build();

                    //if(_authorizationService.AuthorizeAsync(user, item.PersmissionContext, policy).Result)
                    //{
                        filtered.Add(item);
                    //}
                }

                // Process child items
                var oldItems = item.Items;

                item.Items = Reduce(item.Items, user).ToList();
            }

            return filtered;
        }
    }
}
