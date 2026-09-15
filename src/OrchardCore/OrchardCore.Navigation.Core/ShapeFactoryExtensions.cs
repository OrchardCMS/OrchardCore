using Microsoft.AspNetCore.Routing;
using OrchardCore.Navigation;

namespace OrchardCore.DisplayManagement;

public static class ShapeFactoryExtensions
{
    public static ValueTask<IShape> PagerAsync(this IShapeFactory shapeFactory, Pager pager, int totalItemCount)
        => shapeFactory.CreateAsync(nameof(Pager), Arguments.From(new
        {
            pager.Page,
            pager.PageSize,
            TotalItemCount = totalItemCount,
        }));

    public static async ValueTask<IShape> PagerAsync(this IShapeFactory shapeFactory, Pager pager, int totalItemCount, RouteData routeData)
    {
        var pagerShape = await shapeFactory.PagerAsync(pager, totalItemCount);

        if (routeData != null)
        {
            pagerShape.Properties[nameof(RouteData)] = routeData;
        }

        return pagerShape;
    }

    public static ValueTask<IShape> PagerAsync(this IShapeFactory shapeFactory, Pager pager, int totalItemCount, RouteValueDictionary routeValues)
        => shapeFactory.PagerAsync(pager, totalItemCount, routeValues == null ? null : new RouteData(routeValues));

    public static ValueTask<IShape> PagerSlimAsync(this IShapeFactory shapeFactory, PagerSlim pager)
        => shapeFactory.CreateAsync(nameof(PagerSlim), Arguments.From(new
        {
            pager.Before,
            pager.After,
            pager.PageSize,
        }));

    public static async ValueTask<IShape> PagerSlimAsync(this IShapeFactory shapeFactory, PagerSlim pager, IDictionary<string, string> values)
    {
        var shape = await shapeFactory.CreateAsync(nameof(PagerSlim), Arguments.From(new
        {
            pager.Before,
            pager.After,
            pager.PageSize,
        }));

        if (values != null && values.Count > 0)
        {
            shape.Properties["UrlParams"] = values;
        }

        return shape;
    }

    /// <summary>
    /// Creates the <c>Breadcrumb</c> shape of a trail, with one <c>BreadcrumbItem</c> shape per node.
    /// </summary>
    /// <param name="shapeFactory">The shape factory.</param>
    /// <param name="name">The name of the breadcrumb. e.g., <c>ContentsEdit</c>.</param>
    /// <param name="items">The nodes of the trail, as built by the <see cref="IBreadcrumbManager"/>.</param>
    /// <param name="heading">The optional html tag wrapping the text of the current node. e.g., <c>h1</c>.</param>
    /// <param name="displayType">The optional display type of the trail, which becomes an alternate of every shape it renders. e.g., <c>DetailAdmin</c>.</param>
    public static async ValueTask<IShape> BreadcrumbAsync(this IShapeFactory shapeFactory, string name, IEnumerable<BreadcrumbItem> items, string heading = null, string displayType = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(items);

        var breadcrumb = await shapeFactory.CreateAsync("Breadcrumb", Arguments.From(new
        {
            Name = name,
            Heading = heading,
        }));

        if (!string.IsNullOrEmpty(displayType))
        {
            breadcrumb.Metadata.DisplayType = displayType;
        }

        var level = 0;

        foreach (var item in items)
        {
            var itemShape = new BreadcrumbItemViewModel
            {
                Name = name,
                Item = item,
                Breadcrumb = breadcrumb,
                Text = item.Text,
                Href = item.Href,
                IsCurrent = item.IsCurrent,
                Level = level++,
            };

            if (!string.IsNullOrEmpty(displayType))
            {
                itemShape.Metadata.DisplayType = displayType;
            }

            foreach (var className in item.Classes)
            {
                itemShape.Classes.Add(className);
            }

            await breadcrumb.AddAsync(itemShape);
        }

        return breadcrumb;
    }
}
