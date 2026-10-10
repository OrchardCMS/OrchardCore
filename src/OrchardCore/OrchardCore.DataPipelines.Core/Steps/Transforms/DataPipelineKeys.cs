using System.Globalization;
using System.Text;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Builds the keys steps use to group, deduplicate and join rows. Numbers compare by value (<c>2</c> equals
/// <c>2.0</c>), and text compares exactly unless asked to ignore case. Two missing values are equal.
/// </summary>
public static class DataPipelineKeys
{
    private const char Separator = '\u001F';

    /// <summary>
    /// Builds the key of a row.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="indexes">The positions of the key fields.</param>
    /// <param name="ignoreCase">Whether text keys ignore case.</param>
    /// <returns>The key.</returns>
    public static string Build(object[] row, IReadOnlyList<int> indexes, bool ignoreCase)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(indexes);

        var builder = new StringBuilder();

        for (var position = 0; position < indexes.Count; position++)
        {
            if (position > 0)
            {
                builder.Append(Separator);
            }

            var index = indexes[position];
            builder.Append(ToKey(index < row.Length ? row[index] : null, ignoreCase));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Tells whether any key field of a row is missing. A row with a missing key never matches in a join.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="indexes">The positions of the key fields.</param>
    /// <returns><see langword="true"/> when a key field is missing.</returns>
    public static bool HasEmptyKey(object[] row, IReadOnlyList<int> indexes)
        => indexes.Any(index => index >= row.Length || DataValues.IsEmpty(row[index]));

    private static string ToKey(object value, bool ignoreCase)
        => value switch
        {
            null => "\0",
            string text => "s:" + (ignoreCase ? text.ToUpperInvariant() : text),
            bool flag => flag ? "b:1" : "b:0",
            DateTime date => "d:" + date.Ticks.ToString(CultureInfo.InvariantCulture),
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal
                => "n:" + (DataValues.Coerce(value, DataFieldType.Decimal) is decimal number
                    ? number.ToString("G29", CultureInfo.InvariantCulture)
                    : DataValues.ToText(value)),
            _ => "s:" + (ignoreCase ? DataValues.ToText(value)?.ToUpperInvariant() : DataValues.ToText(value)),
        };
}
