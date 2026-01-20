using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class CookieCollectionWrapper
{
    public readonly IRequestCookieCollection RequestCookieCollection;

    public CookieCollectionWrapper(IRequestCookieCollection requestCookieCollection)
    {
        RequestCookieCollection = requestCookieCollection;
    }
}
