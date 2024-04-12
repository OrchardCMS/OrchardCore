using Microsoft.Net.Http.Headers;

namespace OrchardCore.Tests.OrchardCore.Users;

public class CookiesHelper
{
    public static IDictionary<string, string> ExtractCookiesFromResponse(HttpResponseMessage response)
    {
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

    public static HttpRequestMessage PutCookiesOnRequest(HttpRequestMessage request, IDictionary<string, string> cookies)
    {
        foreach (var key in cookies.Keys)
        {
            request.Headers.Add("Cookie", new CookieHeaderValue(key, cookies[key]).ToString());
        }

        return request;
    }

    public static HttpRequestMessage CopyCookiesFromResponse(HttpRequestMessage request, HttpResponseMessage response)
        => PutCookiesOnRequest(request, ExtractCookiesFromResponse(response));
}
