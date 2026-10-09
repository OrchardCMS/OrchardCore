using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers;

/// <summary>
/// Finds the query string parameters and form fields the expressions of a workflow read from the request that started
/// it, such as <c>queryString('id')</c> or <c>{{ Request.QueryString["id"] }}</c>, so the designer's Run dialog can ask
/// for them.
/// </summary>
internal static partial class WorkflowRequestNames
{
    /// <summary>
    /// The names the activities read, in the order they're first read: the query string parameters and the form fields.
    /// </summary>
    public static (IReadOnlyList<string> QueryParameters, IReadOnlyList<string> FormFields) Find(IEnumerable<ActivityRecord> activities)
    {
        var query = new List<string>();
        var form = new List<string>();

        foreach (var activity in activities ?? [])
        {
            Visit(activity.Properties, query, form);
        }

        return (query, form);
    }

    private static void Visit(JsonNode node, List<string> query, List<string> form)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                foreach (var (_, value) in jsonObject)
                {
                    Visit(value, query, form);
                }

                break;

            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                {
                    Visit(item, query, form);
                }

                break;

            case JsonValue value when value.TryGetValue<string>(out var text) && !string.IsNullOrEmpty(text):
                Collect(QueryPattern().Matches(text), query);
                Collect(FormPattern().Matches(text), form);

                break;
        }
    }

    private static void Collect(MatchCollection matches, List<string> names)
    {
        foreach (Match match in matches)
        {
            var name = match.Groups["name"].Value;

            if (!names.Contains(name, StringComparer.Ordinal))
            {
                names.Add(name);
            }
        }
    }

    // queryString('id'), Request.QueryString["id"] and Request.QueryString.id.
    [GeneratedRegex("""(?:\bqueryString\(\s*|\bRequest\.QueryString\[\s*)["'](?<name>[^"']+)["']|\bRequest\.QueryString\.(?<name>[A-Za-z_][\w-]*)""")]
    private static partial Regex QueryPattern();

    // requestForm('id'), Request.Form["id"] and Request.Form.id.
    [GeneratedRegex("""(?:\brequestForm\(\s*|\bRequest\.Form\[\s*)["'](?<name>[^"']+)["']|\bRequest\.Form\.(?<name>[A-Za-z_][\w-]*)""")]
    private static partial Regex FormPattern();
}
