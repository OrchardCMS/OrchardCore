namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class RouteValueDictionaryWrapper
{
    public readonly IReadOnlyDictionary<string, object> RouteValueDictionary;

    public RouteValueDictionaryWrapper(IReadOnlyDictionary<string, object> routeValueDictionary)
    {
        RouteValueDictionary = routeValueDictionary;
    }
}
