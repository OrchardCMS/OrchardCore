using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Renders the names steps give their files and the texts they send, from placeholders: <c>{PipelineName}</c>,
/// <c>{RunId}</c>, <c>{Date:format}</c>, where format is a .NET date format such as <c>yyyy-MM-dd</c>, and
/// <c>{Parameter:name}</c>, a parameter of the run such as one a workflow passes, or nothing when the run has none of
/// that name. The date is when the run started, in the time zone of the site. Unknown placeholders are left as they are.
/// </summary>
public static partial class DataPipelineTemplate
{
    /// <summary>
    /// Renders a text.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="run">The run.</param>
    /// <param name="now">The local date and time the run started.</param>
    /// <returns>The text.</returns>
    public static string Render(string template, DataPipelineRunContext run, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        return PlaceholderPattern().Replace(template, match =>
        {
            var name = match.Groups["name"].Value;
            var format = match.Groups["format"].Success ? match.Groups["format"].Value : null;

            switch (name)
            {
                case "PipelineName":
                    return run.PipelineName ?? string.Empty;

                case "PipelineId":
                    return run.PipelineId ?? string.Empty;

                case "RunId":
                    return run.RunId ?? string.Empty;

                case "Date":
                    try
                    {
                        return now.ToString(string.IsNullOrEmpty(format) ? "yyyy-MM-dd" : format, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        return match.Value;
                    }

                case "Parameter":
                    return format is not null && run.Parameters.TryGetValue(format, out var value)
                        ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
                        : string.Empty;

                default:
                    return match.Value;
            }
        });
    }

    /// <summary>
    /// Renders a file name: the characters a file name can't hold, such as slashes, become underscores.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="run">The run.</param>
    /// <param name="now">The local date and time the run started.</param>
    /// <returns>The file name.</returns>
    public static string RenderFileName(string template, DataPipelineRunContext run, DateTime now)
    {
        var text = Render(template, run, now) ?? string.Empty;
        var builder = new StringBuilder(text.Length);
        var invalid = Path.GetInvalidFileNameChars();

        foreach (var character in text)
        {
            builder.Append(character is '/' or '\\' or ':' or '*' or '?' or '"' or '<' or '>' or '|' || Array.IndexOf(invalid, character) >= 0 ? '_' : character);
        }

        return builder.ToString().Trim().TrimEnd('.');
    }

    /// <summary>
    /// Makes sure a file name ends with an extension.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="extension">The extension, with its leading dot.</param>
    /// <returns>The file name with the extension.</returns>
    public static string WithExtension(string fileName, string extension)
    {
        if (string.IsNullOrEmpty(extension) || fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            return fileName;
        }

        return fileName + extension;
    }

    [GeneratedRegex(@"\{(?<name>[A-Za-z]+)(:(?<format>[^}]+))?\}")]
    private static partial Regex PlaceholderPattern();
}
