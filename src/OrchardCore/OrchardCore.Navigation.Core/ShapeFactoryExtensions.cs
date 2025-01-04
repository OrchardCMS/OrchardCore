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
}
