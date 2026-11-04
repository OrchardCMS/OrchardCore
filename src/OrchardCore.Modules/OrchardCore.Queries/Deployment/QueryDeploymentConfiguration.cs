using System.Text.Json;
using OrchardCore.Localization;

namespace OrchardCore.Queries.Deployment;

internal static class QueryDeploymentConfiguration
{
    public static async Task<Dictionary<string, string[]>> ValidateAsync(IQueryManager queries, string queryName, string parameters)
    {
        var errors = new Dictionary<string, string[]>();
        var query = string.IsNullOrWhiteSpace(queryName) ? null : await queries.GetQueryAsync(queryName);
        if (query is null || !query.ReturnContentItems)
        {
            errors[nameof(QueryBasedContentDeploymentStep.QueryName)] = [LocalizationSource.Create<QueryBasedContentDeploymentStepDriver>("Your Query is not returning content items.").Value];
        }

        if (parameters is not null)
        {
            if (!TryDeserializeParameters(parameters, out var parsed))
            {
                errors[nameof(QueryBasedContentDeploymentStep.QueryParameters)] = [LocalizationSource.Create<QueryBasedContentDeploymentStepDriver>("Something is wrong with your JSON.").Value];
            }
            else if (parsed is null)
            {
                errors[nameof(QueryBasedContentDeploymentStep.QueryParameters)] = [LocalizationSource.Create<QueryBasedContentDeploymentStepDriver>("Make sure it is a valid JSON object. Example: { key : 'value' }").Value];
            }
        }

        return errors;
    }

    // Preserve the parsed null so each existing caller retains its null-object policy.
    public static bool TryDeserializeParameters(string parameters, out Dictionary<string, object> parsed)
    {
        try
        {
            parsed = JConvert.DeserializeObject<Dictionary<string, object>>(parameters);
            return true;
        }
        catch (JsonException)
        {
            parsed = null;
            return false;
        }
    }
}
