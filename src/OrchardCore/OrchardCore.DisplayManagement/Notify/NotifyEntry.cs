using System.Text.Encodings.Web;
using System.Web;
using Cysharp.Text;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Notify;

public enum NotifyType
{
    Success,
    Information,
    Warning,
    Error
}

public class NotifyEntry
{
    private (HtmlEncoder HtmlEncoder, string Message) _cache;

    public NotifyType Type { get; set; }
    public IHtmlContent Message { get; set; }

    public string ToHtmlString(HtmlEncoder htmlEncoder)
    {
        // When the object is created from a cookie the message
        // is an HtmlString so we can use this instead of using
        // the TextWriter path.

        if (Message is IHtmlString htmlString)
        {
            return htmlString.ToHtmlString();
        }

        // Cache the encoded version for the specified encoder.
        // This is necessary as long as there will be string-based comparisons
        // and the need of NotifyEntryComparer

        var cache = _cache;

        if (cache.Message != null && cache.HtmlEncoder == htmlEncoder)
        {
            return cache.Message;
        }

        using var stringWriter = new ZStringWriter();
        Message.WriteTo(stringWriter, htmlEncoder);
        stringWriter.Flush();

        _cache = cache = new(htmlEncoder, stringWriter.ToString());

        return cache.Message;
    }
}
