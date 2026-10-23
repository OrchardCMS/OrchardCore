using System.Text;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Reads and writes the values of a filter that takes a list, such as <see cref="DataSources.DataFilterOperator.In"/>,
/// as one text: the values are separated by commas, and a value that holds a comma or a double quote is written in
/// double quotes, with its double quotes doubled, such as <c>Paris, "Smith, John"</c>.
/// </summary>
public static class DataFilterValueList
{
    /// <summary>
    /// Splits a text into its values. Each value is trimmed, and empty values are dropped.
    /// </summary>
    /// <param name="text">The text, such as <c>Paris, "Smith, John"</c>.</param>
    /// <returns>The values.</returns>
    public static List<string> Parse(string text)
    {
        var values = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
        {
            return values;
        }

        var value = new StringBuilder();
        var quoted = false;
        var inQuotes = false;

        void Add()
        {
            var item = quoted ? value.ToString() : value.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(item))
            {
                values.Add(item);
            }

            value.Clear();
            quoted = false;
        }

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];

            if (inQuotes)
            {
                if (character == '"' && index + 1 < text.Length && text[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                }
                else if (character == '"')
                {
                    inQuotes = false;
                }
                else
                {
                    value.Append(character);
                }
            }
            else if (character == ',')
            {
                Add();
            }
            else if (character == '"' && string.IsNullOrWhiteSpace(value.ToString()))
            {
                // A quote opens a quoted value only at its start; the spaces before it are dropped.
                value.Clear();
                inQuotes = true;
                quoted = true;
            }
            else if (!quoted || !char.IsWhiteSpace(character))
            {
                value.Append(character);
            }
        }

        Add();

        return values;
    }

    /// <summary>
    /// Joins values into one text that <see cref="Parse(string)"/> reads back.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The text, such as <c>Paris, "Smith, John"</c>.</returns>
    public static string Format(IEnumerable<string> values)
    {
        return string.Join(", ", (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Contains(',', StringComparison.Ordinal) || value.Contains('"', StringComparison.Ordinal) || value.Trim() != value
                ? "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
                : value));
    }
}
