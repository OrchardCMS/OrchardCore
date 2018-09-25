using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Navigation
{
    public class NavigationBuilder
    {
        List<MenuItem> Contained { get; set; }

        public NavigationBuilder()
        {
            Contained = new List<MenuItem>();
        }

        public NavigationBuilder Add(LocalizedString caption, string position, Action<NavigationItemBuilder> itemBuilder, IEnumerable<string> classes = null)
        {
            var childBuilder = new NavigationItemBuilder();

            childBuilder.Caption(caption);
            childBuilder.Position(position);
            itemBuilder(childBuilder);
            Contained.AddRange(childBuilder.Build());

            if (classes != null)
            {
                foreach (var className in classes)
                    childBuilder.AddClass(className);
            }

            return this;
        }

        public NavigationBuilder Add(LocalizedString caption, Action<NavigationItemBuilder> itemBuilder, IEnumerable<string> classes = null)
        {
            return Add(caption, null, itemBuilder, classes);
        }
        public NavigationBuilder Add(Action<NavigationItemBuilder> itemBuilder, IEnumerable<string> classes = null)
        {
            return Add(new LocalizedString(null, null), null, itemBuilder, classes);
        }
        public NavigationBuilder Add(LocalizedString caption, string position, IEnumerable<string> classes = null)
        {
            return Add(caption, position, x => { }, classes);
        }
        public NavigationBuilder Add(LocalizedString caption, IEnumerable<string> classes = null)
        {
            return Add(caption, null, x => { }, classes);
        }

        public NavigationBuilder Remove(MenuItem item)
        {
            Contained.Remove(item);
            return this;
        }

        public NavigationBuilder Remove(Predicate<MenuItem> match)
        {
            RemoveRecursive(Contained, match);
            return this;
        }

        public virtual List<MenuItem> Build()
        {
            return (Contained ?? new List<MenuItem>()).ToList();
        }

        private static void RemoveRecursive(List<MenuItem> menuItems, Predicate<MenuItem> match)
        {
            menuItems.RemoveAll(match);
            foreach (var menuItem in menuItems)
            {
                if (menuItem.Items?.Count > 0)
                {
                    RemoveRecursive(menuItem.Items, match);
                }
            }
        }
    }
}
