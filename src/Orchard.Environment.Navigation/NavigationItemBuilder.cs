using Microsoft.AspNet.Routing;
using Microsoft.Extensions.Localization;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Navigation
{
    public class NavigationItemBuilder : NavigationBuilder
    {
        private readonly MenuItem _item;

        public NavigationItemBuilder()
        {
            _item = new MenuItem();
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

        public NavigationItemBuilder IdHint(string idHint)
        {
            _item.IdHint = idHint;
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

        public NavigationItemBuilder Permission(Permission permission)
        {
            _item.Permissions = _item.Permissions.Concat(new[] { permission });
            return this;
        }

        public new IEnumerable<MenuItem> Build()
        {
            _item.Items = base.Build();
            return new[] { _item };
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
            _item.RouteValues = new RouteValueDictionary(values);
            if (!string.IsNullOrEmpty(actionName))
                _item.RouteValues["action"] = actionName;
            if (!string.IsNullOrEmpty(controllerName))
                _item.RouteValues["controller"] = controllerName;
            return this;
        }
    }
}