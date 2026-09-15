using Microsoft.AspNetCore.Routing;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Navigation;

/// <summary>
/// Configures a single <see cref="BreadcrumbItem"/> of a breadcrumb trail.
/// </summary>
public sealed class BreadcrumbItemBuilder
{
    private readonly BreadcrumbItem _item;

    public BreadcrumbItemBuilder(BreadcrumbItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _item = item;
    }

    /// <summary>
    /// Sets the text to display for the node.
    /// </summary>
    public BreadcrumbItemBuilder Text(string text)
    {
        _item.Text = text;

        return this;
    }

    /// <summary>
    /// Sets the identifier of the node, which is used to build its shape alternates.
    /// </summary>
    public BreadcrumbItemBuilder Id(string id)
    {
        _item.Id = id;

        return this;
    }

    /// <summary>
    /// Sets the relative position of the node among the other nodes of the trail.
    /// </summary>
    public BreadcrumbItemBuilder Position(string position)
    {
        _item.Position = position;

        return this;
    }

    /// <summary>
    /// Sets the url the node links to.
    /// </summary>
    public BreadcrumbItemBuilder Url(string url)
    {
        _item.Url = url;

        return this;
    }

    /// <summary>
    /// Adds a css class to render with the node.
    /// </summary>
    public BreadcrumbItemBuilder AddClass(string className)
    {
        if (!_item.Classes.Contains(className))
        {
            _item.Classes.Add(className);
        }

        return this;
    }

    /// <summary>
    /// Removes a css class from the node.
    /// </summary>
    public BreadcrumbItemBuilder RemoveClass(string className)
    {
        _item.Classes.Remove(className);

        return this;
    }

    /// <summary>
    /// Adds a permission the user must have for the node to be rendered as a link.
    /// </summary>
    public BreadcrumbItemBuilder Permission(Permission permission)
    {
        _item.Permissions.Add(permission);

        return this;
    }

    /// <summary>
    /// Adds the permissions the user must all have for the node to be rendered as a link.
    /// </summary>
    public BreadcrumbItemBuilder Permissions(IEnumerable<Permission> permissions)
    {
        _item.Permissions.AddRange(permissions);

        return this;
    }

    /// <summary>
    /// Sets the resource the permissions of the node are evaluated against.
    /// </summary>
    public BreadcrumbItemBuilder Resource(object resource)
    {
        _item.Resource = resource;

        return this;
    }

    public BreadcrumbItemBuilder Action(string actionName)
        => Action(actionName, null, null, []);

    public BreadcrumbItemBuilder Action(string actionName, string controllerName)
        => Action(actionName, controllerName, null, []);

    public BreadcrumbItemBuilder Action(string actionName, string controllerName, object values)
        => Action(actionName, controllerName, null, new RouteValueDictionary(values));

    public BreadcrumbItemBuilder Action(string actionName, string controllerName, RouteValueDictionary values)
        => Action(actionName, controllerName, null, values);

    public BreadcrumbItemBuilder Action(string actionName, string controllerName, string areaName)
        => Action(actionName, controllerName, areaName, []);

    public BreadcrumbItemBuilder Action(string actionName, string controllerName, string areaName, RouteValueDictionary values)
    {
        _item.RouteValues = new RouteValueDictionary(values);

        if (!string.IsNullOrEmpty(actionName))
        {
            _item.RouteValues["action"] = actionName;
        }

        if (!string.IsNullOrEmpty(controllerName))
        {
            _item.RouteValues["controller"] = controllerName;
        }

        if (!string.IsNullOrEmpty(areaName))
        {
            _item.RouteValues["area"] = areaName;
        }

        return this;
    }
}
