using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Html;

public class CssHelpers
{
    public static readonly HtmlString CssClassSeperator = new(" ");

    public static void AppendValue(HtmlContentBuilder builder, string value)
    {
        if (builder.Count == 0)
        {
            // The 'Append' method performs a string.IsNullOrEmpty() check before appending the value.
            // So, as long as the builder has no entries, we append with no spaces.
            builder.Append(value);

            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        // At this point, we already know that the builder has at least one entry, so we append a single space to the class name.
        // We pass create HtmlString here to prevent the builder from preforming string.IsNullOrWhiteSpace again for performance reason.
        builder.AppendHtml(CssClassSeperator);

        // We use Append here to ensure that the value is encoded.
        builder.Append(value);
    }
}
