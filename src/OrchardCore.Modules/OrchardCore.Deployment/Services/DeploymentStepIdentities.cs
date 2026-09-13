using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Deployment.Services;

internal static class DeploymentStepIdentities
{
    // Reserve every supplied ID before generating or reusing any missing one.
    internal static bool EnsureUnique(IReadOnlyList<DeploymentStep> steps, IReadOnlyList<DeploymentStep> previous = null)
    {
        var reserved = steps.Where(step => !string.IsNullOrWhiteSpace(step.Id))
            .Select(step => step.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var changed = false;
        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            if (!string.IsNullOrWhiteSpace(step.Id) && seen.Add(step.Id))
            {
                continue;
            }

            string id = null;
            // Recipes without IDs retain the identity of the same kind of step in the same slot.
            if (previous is not null && index < previous.Count)
            {
                var prior = previous[index];
                if (prior.GetType() == step.GetType() && prior.Name == step.Name
                    && !string.IsNullOrWhiteSpace(prior.Id) && reserved.Add(prior.Id))
                {
                    id = prior.Id;
                }
            }
            if (id is null)
            {
                do { id = Guid.NewGuid().ToString("n"); } while (!reserved.Add(id));
            }
            seen.Add(id);
            step.Id = id;
            if (step is UnknownDeploymentStep unknown && unknown.RawData.ValueKind == JsonValueKind.Object)
            {
                // The fallback serializer writes RawData, so update only its identity field too.
                var data = JsonNode.Parse(unknown.RawData.GetRawText()).AsObject();
                var key = data.Select(property => property.Key)
                    .FirstOrDefault(key => string.Equals(key, nameof(DeploymentStep.Id), StringComparison.OrdinalIgnoreCase))
                    ?? nameof(DeploymentStep.Id);
                data[key] = id;
                unknown.RawData = JsonSerializer.SerializeToElement(data);
            }
            changed = true;
        }
        return changed;
    }
}
