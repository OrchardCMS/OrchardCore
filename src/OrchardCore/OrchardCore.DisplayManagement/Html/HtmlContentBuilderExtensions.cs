using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Html;

public static class HtmlContentBuilderExtensions
{
    private static readonly HtmlString _whitespace = new(" ");
    private static readonly HtmlString _hyphen = new("-");

    public static HtmlContentBuilder AppendSeparatedValue(this HtmlContentBuilder builder, string value)
    {
        if (builder.Count == 0)
        {
            // The 'Append' method performs a string.IsNullOrEmpty() check before appending the value.
            // So, as long as the builder has no entries, we append with no spaces.
            builder.Append(value);

            return builder;
        }

        if (string.IsNullOrEmpty(value))
        {
            return builder;
        }

        // At this point, we already know that the builder has at least one entry, so we append a single space to the class name.
        // We pass create 'HtmlString' here to prevent the builder from preforming string.IsNullOrWhiteSpace again for performance reason.
        builder.AppendWhitespace();

        // We use 'Append' here to ensure that the value is encoded.
        builder.Append(value);

        return builder;
    }

    public static HtmlContentBuilder AppendWhitespace(this HtmlContentBuilder builder)
    {
        builder.AppendHtml(_whitespace);

        return builder;
    }

    public static HtmlContentBuilder AppendHyphen(this HtmlContentBuilder builder)
    {
        builder.AppendHtml(_hyphen);

        return builder;
    }

    public static HtmlContentBuilder AppendSeparatedValues(this HtmlContentBuilder builder, IList<string> values)
    {
        if (values?.Count > 0)
        {
            foreach (var value in values)
            {
                builder.AppendSeparatedValue(value);
            }
        }

        return builder;
    }
}
