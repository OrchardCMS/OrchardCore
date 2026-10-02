using System.Globalization;
using System.Text;

namespace OrchardCore.Localization.Extraction;

public static class PotWriter
{
    public static string Write(LocalizationCatalog catalog, string assemblyName)
    {
        var output = new StringBuilder();
        output.Append("msgid \"\"\nmsgstr \"\"\n");
        AppendHeader(output, "Project-Id-Version: " + assemblyName);
        AppendHeader(output, "MIME-Version: 1.0");
        AppendHeader(output, "Content-Type: text/plain; charset=UTF-8");
        AppendHeader(output, "Content-Transfer-Encoding: 8bit");
        AppendHeader(output, "X-OrchardCore-Catalog-Schema: 1");

        foreach (var message in catalog.Messages.OrderBy(message => message.Context, StringComparer.Ordinal).ThenBy(message => message.Text, StringComparer.Ordinal))
        {
            output.Append('\n');
            foreach (var comment in message.Comments)
            {
                foreach (var line in comment.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
                {
                    output.Append("#. ").Append(line).Append('\n');
                }
            }

            foreach (var reference in message.References.OrderBy(reference => reference.Path, StringComparer.Ordinal).ThenBy(reference => reference.Line))
            {
                var path = reference.Path.Any(char.IsWhiteSpace) ? "\u2068" + reference.Path + "\u2069" : reference.Path;
                output.Append("#: ").Append(path);
                if (reference.Line > 0)
                {
                    output.Append(':').Append(reference.Line.ToString(CultureInfo.InvariantCulture));
                }

                output.Append('\n');
            }

            var flags = new SortedSet<string>(message.Flags, StringComparer.Ordinal);
            if (HasFormat(message.Text) || message.Plural is not null && HasFormat(message.Plural))
            {
                flags.Add("csharp-format");
            }

            if (flags.Count > 0)
            {
                output.Append("#, ").AppendJoin(", ", flags).Append('\n');
            }

            AppendField(output, "msgctxt", message.Context);
            AppendField(output, "msgid", message.Text);
            if (message.Plural is null)
            {
                output.Append("msgstr \"\"\n");
            }
            else
            {
                AppendField(output, "msgid_plural", message.Plural);
                output.Append("msgstr[0] \"\"\nmsgstr[1] \"\"\n");
            }
        }

        return output.ToString();
    }

    public static async Task<bool> WriteIfChangedAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        if (File.Exists(path) && string.Equals(await File.ReadAllTextAsync(path, cancellationToken), content, StringComparison.Ordinal))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, content, new UTF8Encoding(false), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }

        return true;
    }

    private static void AppendHeader(StringBuilder output, string value)
        => output.Append('"').Append(Escape(value + "\n")).Append("\"\n");

    private static void AppendField(StringBuilder output, string name, string value)
        => output.Append(name).Append(" \"").Append(Escape(value)).Append("\"\n");

    private static string Escape(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);

    private static bool HasFormat(string value)
    {
        try
        {
            return CompositeFormat.Parse(value).MinimumArgumentCount > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
