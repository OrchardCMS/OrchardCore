namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class HttpContextItemsWrapper
{
    public readonly IDictionary<object, object> Items;

    public HttpContextItemsWrapper(IDictionary<object, object> items)
    {
        Items = items;
    }
}
