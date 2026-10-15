using System.Globalization;
using System.Text;
using System.Text.Json;
using OrchardCore.DataLocalization.Models;

namespace OrchardCore.DataLocalization.Services;

/// <summary>
/// Reads catalogs including empty translations and metadata, and writes override PO files.
/// Unlike the runtime PO parser, catalog discovery must retain untranslated identifiers.
/// </summary>
public static class UiPortableObject
{
    /// <summary>Reads a POT or PO document, rejecting malformed fields and plural indices.</summary>
    /// <param name="reader">The input reader.</param>
    /// <param name="assemblyName">The catalog's source assembly name.</param>
    public static IReadOnlyList<UiLocalizationResource> Read(TextReader reader, string assemblyName)
    {
        var results = new List<UiLocalizationResource>();
        var entry = new UiLocalizationResource { AssemblyName = assemblyName };
        string field = null;
        var index = -1;
        var fields = new HashSet<string>(StringComparer.Ordinal);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0)
            {
                Flush();
                continue;
            }

            if (line.StartsWith('#'))
            {
                if (entry.Values.Count > 0)
                {
                    Flush();
                }

                entry.Metadata.Add(line);
                continue;
            }

            var continuation = line.StartsWith('"');
            if (!continuation)
            {
                var separator = line.IndexOf(' ');
                if (separator < 0)
                {
                    throw new FormatException("A portable object field must have a quoted value.");
                }

                var nextField = line[..separator];
                if (entry.Values.Count > 0 && nextField is "msgctxt" or "msgid")
                {
                    Flush();
                }

                field = nextField;
                if (!fields.Add(field))
                {
                    throw new FormatException("Duplicate portable object field: " + field);
                }

                index = -1;
                if (field.StartsWith("msgstr[", StringComparison.Ordinal))
                {
                    if (!field.EndsWith(']') || !int.TryParse(field.AsSpan(7, field.Length - 8), NumberStyles.None, CultureInfo.InvariantCulture, out index) ||
                        index != entry.Values.Count || fields.Contains("msgstr"))
                    {
                        throw new FormatException("Plural translations must have consecutive indices starting at zero.");
                    }
                }
                else if (field is not ("msgctxt" or "msgid" or "msgid_plural" or "msgstr"))
                {
                    throw new FormatException("Unknown portable object field: " + field);
                }

                line = line[(separator + 1)..].Trim();
            }

            string value;
            try
            {
                value = JsonSerializer.Deserialize<string>(line) ?? throw new FormatException("A portable object value cannot be null.");
            }
            catch (JsonException exception)
            {
                throw new FormatException("Invalid portable object quoted string.", exception);
            }

            switch (field)
            {
                case "msgctxt": entry.Context = (entry.Context ?? "") + value; break;
                case "msgid": entry.Key = (entry.Key ?? "") + value; break;
                case "msgid_plural": entry.Plural = (entry.Plural ?? "") + value; break;
                case "msgstr":
                    if (!continuation)
                    {
                        if (entry.Values.Count != 0)
                        {
                            throw new FormatException("Singular and plural translations cannot be mixed.");
                        }

                        entry.Values.Add(value);
                    }
                    else
                    {
                        entry.Values[0] += value;
                    }

                    break;
                default:
                    if (index < 0)
                    {
                        throw new FormatException("A continuation must follow a portable object field.");
                    }

                    if (continuation)
                    {
                        entry.Values[index] += value;
                    }
                    else
                    {
                        entry.Values.Add(value);
                    }

                    break;
            }
        }

        Flush();
        return results;

        void Flush()
        {
            if (fields.Count > 0)
            {
                if (entry.Key == null || entry.Values.Count == 0)
                {
                    throw new FormatException("An entry must have msgid and msgstr fields.");
                }

                if (entry.Key.Length > 0)
                {
                    entry.Context ??= "";
                    results.Add(entry);
                }
            }

            entry = new UiLocalizationResource { AssemblyName = assemblyName };
            fields.Clear();
            field = null;
            index = -1;
        }
    }

    /// <summary>Exports only persisted overrides, retaining contexts and plural source identifiers.</summary>
    /// <param name="culture">The culture name.</param>
    /// <param name="translations">The overrides.</param>
    public static string Write(string culture, IEnumerable<UiTranslation> translations)
    {
        var output = new StringBuilder();
        output.Append("msgid \"\"\nmsgstr \"\"\n");
        output.Append(Quote("Language: " + culture + "\n")).Append('\n');
        output.Append(Quote("Content-Type: text/plain; charset=UTF-8\n")).Append('\n');
        foreach (var translation in translations.OrderBy(x => x.Context, StringComparer.Ordinal).ThenBy(x => x.Key, StringComparer.Ordinal))
        {
            output.Append('\n');
            Field("msgctxt", translation.Context);
            Field("msgid", translation.Key);
            if (translation.Plural != null)
            {
                Field("msgid_plural", translation.Plural);
            }

            for (var i = 0; i < translation.Values.Length; i++)
            {
                Field(translation.Plural == null ? "msgstr" : "msgstr[" + i.ToString(CultureInfo.InvariantCulture) + "]", translation.Values[i]);
            }
        }

        return output.ToString();

        void Field(string name, string value) => output.Append(name).Append(' ').Append(Quote(value)).Append('\n');
    }

    private static string Quote(string value)
        => "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal).Replace("\t", "\\t", StringComparison.Ordinal) + "\"";
}
