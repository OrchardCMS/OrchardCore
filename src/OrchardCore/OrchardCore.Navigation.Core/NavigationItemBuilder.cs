using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Navigation
{
    public class NavigationItemBuilder : NavigationBuilder
    {
        private readonly MenuItem _item;

        public NavigationItemBuilder()
        {
            _item = new MenuItem();
        }

        public NavigationItemBuilder(MenuItem existingItem)
        {
            if (existingItem == null)
            {
                throw new ArgumentNullException(nameof(existingItem));
            }

            _item = existingItem;
        }

        public NavigationItemBuilder Caption(LocalizedString caption)
        {
            _item.Text = caption;
            return this;
        }

        public NavigationItemBuilder Position(string position)
        {
            _item.Position = position;
            return this;
        }

        public NavigationItemBuilder Priority(int priority)
        {
            _item.Priority = priority;
            return this;
        }

        public NavigationItemBuilder Url(string url)
        {
            _item.Url = url;
            return this;
        }

        public NavigationItemBuilder Culture(string culture)
        {
            _item.Culture = culture;
            return this;
        }

        public NavigationItemBuilder Id(string id)
        {
            _item.Id = id;
            return this;
        }

        public NavigationItemBuilder AddClass(string className)
        {
            if (!_item.Classes.Contains(className))
                _item.Classes.Add(className);
            return this;
        }

        public NavigationItemBuilder RemoveClass(string className)
        {
            if (_item.Classes.Contains(className))
                _item.Classes.Remove(className);
            return this;
        }

        public NavigationItemBuilder LinkToFirstChild(bool value)
        {
            _item.LinkToFirstChild = value;
            return this;
        }

        public NavigationItemBuilder LocalNav()
        {
            _item.LocalNav = true;
            return this;
        }

        public NavigationItemBuilder Local(bool value)
        {
            _item.LocalNav = value;
            return this;
        }

        public NavigationItemBuilder Permission(Permission permission)
        {
            _item.Permissions.Add(permission);
            return this;
        }

        public NavigationItemBuilder Permissions(IEnumerable<Permission> permissions)
        {
            _item.Permissions.AddRange(permissions);
            return this;
        }

        public NavigationItemBuilder Resource(object resource)
        {
            _item.Resource = resource;
            return this;
        }

        public override List<MenuItem> Build()
        {
            _item.Items = base.Build();
            return new List<MenuItem> { _item };
        }

        public NavigationItemBuilder Action(RouteValueDictionary values)
        {
            return values != null
                       ? Action(values["action"] as string, values["controller"] as string, values)
                       : Action(null, null, new RouteValueDictionary());
        }

        public NavigationItemBuilder Action(string actionName)
        {
            return Action(actionName, null, new RouteValueDictionary());
        }

        public NavigationItemBuilder Action(string actionName, string controllerName)
        {
            return Action(actionName, controllerName, new RouteValueDictionary());
        }

        public NavigationItemBuilder Action(string actionName, string controllerName, object values)
        {
            return Action(actionName, controllerName, new RouteValueDictionary(values));
        }

        public NavigationItemBuilder Action(string actionName, string controllerName, RouteValueDictionary values)
        {
            return Action(actionName, controllerName, null, values);
        }

        public NavigationItemBuilder Action(string actionName, string controllerName, string areaName)
        {
            return Action(actionName, controllerName, areaName, new RouteValueDictionary());
        }

        public NavigationItemBuilder Action(string actionName, string controllerName, string areaName, RouteValueDictionary values)
        {
            _item.RouteValues = new RouteValueDictionary(values);
            if (!String.IsNullOrEmpty(actionName))
            {
                _item.RouteValues["action"] = actionName;
            }

            if (!String.IsNullOrEmpty(controllerName))
            {
                _item.RouteValues["controller"] = controllerName;
            }

            if (!String.IsNullOrEmpty(areaName))
            {
                _item.RouteValues["area"] = areaName;
            }

            return this;
        }
    }
}
