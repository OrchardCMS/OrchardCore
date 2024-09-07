using Microsoft.Net.Http.Headers;

namespace OrchardCore.Tests.OrchardCore.Users;

public static class CookiesHelper
{
    public static IDictionary<string, string> ExtractCookies(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var result = new Dictionary<string, string>();

        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            foreach (var cookie in SetCookieHeaderValue.ParseList(values.ToList()))
            {
                result.Add(cookie.Name.ToString(), cookie.Value.ToString());
            }
        }

        return result;
    }

    public static HttpRequestMessage AddCookiesToRequest(HttpRequestMessage request, IDictionary<string, string> cookies)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(cookies);

        foreach (var key in cookies.Keys)
        {
            request.Headers.Add("Cookie", new CookieHeaderValue(key, cookies[key]).ToString());
        }

        return request;
    }

    public static HttpRequestMessage CopyCookies(HttpRequestMessage source, HttpResponseMessage destination)
        => AddCookiesToRequest(source, ExtractCookies(destination));
}
