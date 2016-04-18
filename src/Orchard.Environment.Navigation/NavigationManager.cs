using Orchard.Environment.Shell;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

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

            // Remove unauthorized menu items
            menuItems = Reduce(menuItems, null);

            // Organize menu items hierarchy
            menuItems = Arrange(menuItems);

            // Compute Url and RouteValues properties to Href
            menuItems = ComputeHref(menuItems, actionContext);

            return menuItems;
        }

        /// <summary>
        /// Organizes a list of <see cref="MenuItem"/> into a hierarchy based on their positions
        /// </summary>
        private static IEnumerable<MenuItem> Arrange(IEnumerable<MenuItem> items)
        {
            var result = new List<MenuItem>();
            var index = new Dictionary<string, MenuItem>();

            foreach (var item in items)
            {
                MenuItem parent;
                var parentPosition = String.Empty;

                var position = item.Position ?? String.Empty;

                var lastSegment = position.LastIndexOf('.');
                if (lastSegment != -1)
                {
                    parentPosition = position.Substring(0, lastSegment);
                }

                if (index.TryGetValue(parentPosition, out parent))
                {
                    parent.Items = parent.Items.Concat(new[] { item });
                }
                else
                {
                    result.Add(item);
                }

                if (!index.ContainsKey(position))
                {
                    // Prevent invalid positions
                    index.Add(position, item);
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the <see cref="MenuItem.Href"/> properties based on <see cref="MenuItem.Url"/>
        /// and <see cref="MenuItem.RouteValues"/> values.
        /// </summary>
        private IEnumerable<MenuItem> ComputeHref(IEnumerable<MenuItem> menuItems, ActionContext actionContext)
        {
            foreach (var menuItem in menuItems)
            {
                menuItem.Href = GetUrl(menuItem.Url, menuItem.RouteValues, actionContext);
                menuItem.Items = ComputeHref(menuItem.Items.ToArray(), actionContext);
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
        private IEnumerable<MenuItem> Reduce(IEnumerable<MenuItem> items, ClaimsPrincipal user)
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

                // if all sub items have been filtered out, ensure the main one is not one of them
                // e.g., Manage Roles and Manage Users are not granted, the Users item should not show up
                if (oldItems.Any() && !item.Items.Any())
                {
                    if (oldItems.Any(x => NavigationHelper.RouteMatches(x.RouteValues, item.RouteValues)))
                    {
                        continue;
                    }
                }
            }

            return filtered;
        }

    }
}
