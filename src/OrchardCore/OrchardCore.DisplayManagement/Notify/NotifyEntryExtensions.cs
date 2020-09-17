using System.IO;
using System.Text;
using System.Text.Encodings.Web;

namespace OrchardCore.DisplayManagement.Notify
{
    internal static class NotifyEntryExtensions
    {
        public static string GetMessageAsString(this NotifyEntry entry, HtmlEncoder htmlEncoder)
        {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                entry.Message.WriteTo(stringWriter, htmlEncoder);
                stringWriter.Flush();
            }

            return stringBuilder.ToString();
        }
    }
}
