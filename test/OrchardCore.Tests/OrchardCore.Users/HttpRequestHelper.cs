namespace OrchardCore.Tests.OrchardCore.Users;

public static class HttpRequestHelper
{
    public static HttpRequestMessage CreatePost(string path, Dictionary<string, string> data)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(data);

        return new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(ToFormPostData(data))
        };
    }

    public static HttpRequestMessage CreateGet(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return new HttpRequestMessage(HttpMethod.Get, path);
    }

    public static HttpRequestMessage CreatePostMessageWithCookies(string path, Dictionary<string, string> data, HttpResponseMessage response)
    {
        var message = CreatePost(path, data);

        return CookiesHelper.CopyCookies(message, response);
    }

    public static HttpRequestMessage CreateGetMessageWithCookies(string path, HttpResponseMessage response)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var message = CreateGet(path);

        return CookiesHelper.CopyCookies(message, response);
    }

    private static List<KeyValuePair<string, string>> ToFormPostData(Dictionary<string, string> data)
    {
        var result = new List<KeyValuePair<string, string>>();

        foreach (var key in data.Keys)
        {
            result.Add(new KeyValuePair<string, string>(key, data[key]));
        }

        return result;
    }
}
