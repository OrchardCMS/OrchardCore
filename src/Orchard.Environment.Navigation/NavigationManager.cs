using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Navigation
{
    public class NavigationManager : INavigationManager
    {
        private readonly IEnumerable<INavigationProvider> _navigationProviders;
        private readonly ILogger _logger;

        public NavigationManager(
            IEnumerable<INavigationProvider> navigationProviders,
            ILogger<NavigationManager> logger)
        {
            _navigationProviders = navigationProviders;
            _logger = logger;
        }

        public IEnumerable<MenuItem> BuildMenu(string name)
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
    }
}
