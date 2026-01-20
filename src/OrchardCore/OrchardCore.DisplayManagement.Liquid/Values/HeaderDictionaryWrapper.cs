using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class HeaderDictionaryWrapper
{
    public readonly IHeaderDictionary HeaderDictionary;

    public HeaderDictionaryWrapper(IHeaderDictionary headerDictionary)
    {
        HeaderDictionary = headerDictionary;
    }
}
