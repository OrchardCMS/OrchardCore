namespace OrchardCore.Tests.OrchardCore.Users;

public static class PostRequestHelper
{
    public static HttpRequestMessage Create(string path, Dictionary<string, string> data)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(data);

        var message = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(ToFormPostData(data))
        };

        return message;
    }

    public static HttpRequestMessage CreateMessageWithCookies(string path, Dictionary<string, string> data, HttpResponseMessage response)
    {
        var message = Create(path, data);

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
