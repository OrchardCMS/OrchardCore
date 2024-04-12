namespace OrchardCore.Tests.OrchardCore.Users;

public class PostRequestHelper
{
    public static HttpRequestMessage Create(string path, Dictionary<string, string> formPostBodyData)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(ToFormPostData(formPostBodyData))
        };

        return httpRequestMessage;
    }

    public static HttpRequestMessage CreateWithCookiesFromResponse(string path, Dictionary<string, string> formPostBodyData,
        HttpResponseMessage response)
    {
        var httpRequestMessage = Create(path, formPostBodyData);
        return CookiesHelper.CopyCookiesFromResponse(httpRequestMessage, response);
    }

    private static List<KeyValuePair<string, string>> ToFormPostData(Dictionary<string, string> formPostBodyData)
    {
        var result = new List<KeyValuePair<string, string>>();
        formPostBodyData.Keys.ToList().ForEach(key =>
        {
            result.Add(new KeyValuePair<string, string>(key, formPostBodyData[key]));
        });
        return result;
    }
}
