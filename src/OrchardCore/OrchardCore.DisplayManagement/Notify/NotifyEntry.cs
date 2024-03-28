using System.Text.Encodings.Web;
using Cysharp.Text;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Notify
{
    public enum NotifyType
    {
        Success,
        Information,
        Warning,
        Error
    }

    public class NotifyEntry
    {
        private HtmlEncoder _htmlEncoder;
        private string _encodedMessage;

        public NotifyType Type { get; set; }
        public IHtmlContent Message { get; set; }

        public string ToString(HtmlEncoder htmlEncoder)
        {
            // Cache the encoded version for the specified encoder.
            // This is necessary as long as there will be string-based comparisons
            // and the need of NotifyEntryComparer

            if (_encodedMessage != null && _htmlEncoder == htmlEncoder)
            {
                return _encodedMessage;
            }

            using var stringWriter = new ZStringWriter();
            Message.WriteTo(stringWriter, htmlEncoder);
            stringWriter.Flush();

            _htmlEncoder = htmlEncoder;
            return _encodedMessage = stringWriter.ToString();
        }
    }
}
