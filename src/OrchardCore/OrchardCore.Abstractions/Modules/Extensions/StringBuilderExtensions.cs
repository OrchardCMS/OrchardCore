using System.Text;

namespace OrchardCore.Modules.Extensions;

public static class StringBuilderExtensions
{
    private const char _comma = ',';

    public static StringBuilder AppendComma(this StringBuilder builder)
    {
        builder.Append(_comma);

        return builder;
    }

    public static StringBuilder AppendCommaSeparatedValues(this StringBuilder builder, params string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return builder;
        }

        foreach (var value in values)
        {
            if (value == null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendComma();
            }

            builder.Append(value);
        }

        return builder;
    }

    public static StringBuilder AppendCommaSeparatedValues(this StringBuilder builder, params char[] values)
    {
        if (values == null || values.Length == 0)
        {
            return builder;
        }

        foreach (var value in values)
        {
            if (builder.Length > 0)
            {
                builder.AppendComma();
            }

            builder.Append(value);
        }

        return builder;
    }
}
