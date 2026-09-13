using System.Text.Json;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Deployment.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Deployment;

public class DeploymentPlanService : IDeploymentPlanService
{
    private readonly YesSql.ISession _session;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private Dictionary<string, DeploymentPlan> _deploymentPlans;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>Creates the tenant plan service with document serialization for detached editors.</summary>
    public DeploymentPlanService(
        YesSql.ISession session,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<DocumentJsonSerializerOptions> jsonOptions)
    {
        _session = session;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _jsonOptions = jsonOptions.Value.SerializerOptions;
    }

    /// <inheritdoc />
    public Task<DeploymentPlan> GetAsync(long id) => _session.GetAsync<DeploymentPlan>(id);

    /// <inheritdoc />
    public Task<DeploymentPlan> FindByNameAsync(string name) =>
        _session.Query<DeploymentPlan, DeploymentPlanIndex>(index => index.Name == name).FirstOrDefaultAsync();

    /// <inheritdoc />
    public async Task<DeploymentPlanPage> ListAsync(string search, int skip, int take)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);
        var query = _session.Query<DeploymentPlan, DeploymentPlanIndex>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(index => index.Name.Contains(search));
        }

        var count = await query.CountAsync();
        var items = await query.OrderBy(index => index.Name).ThenBy(index => index.DocumentId)
            .Skip(skip).Take(take).ListAsync();
        return new DeploymentPlanPage { TotalCount = count, Items = items.ToArray() };
    }

    /// <inheritdoc />
    public async Task<DeploymentPlanManagementResult> CreateAsync(string name)
    {
        var error = await ValidateNameAsync(name, 0);
        if (error != DeploymentPlanManagementError.None)
        {
            return new() { Error = error };
        }

        var plan = new DeploymentPlan { Name = name };
        await SaveAsync(plan);
        return new() { Plan = plan, Changed = true };
    }

    /// <inheritdoc />
    public async Task<DeploymentPlanManagementResult> RenameAsync(long id, string name)
    {
        var plan = await GetAsync(id);
        if (plan is null)
        {
            return new() { Error = DeploymentPlanManagementError.NotFound };
        }
        if (plan.Name == name)
        {
            return new() { Plan = plan };
        }

        var error = await ValidateNameAsync(name, id);
        if (error != DeploymentPlanManagementError.None)
        {
            return new() { Error = error };
        }

        plan.Name = name;
        await SaveAsync(plan);
        return new() { Plan = plan, Changed = true };
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id)
    {
        var plan = await GetAsync(id);
        if (plan is null)
        {
            return false;
        }
        _session.Delete(plan);
        _deploymentPlans = null;
        return true;
    }

    private async Task<DeploymentPlanManagementError> ValidateNameAsync(string name, long id)
    {
        if (!ValidName(name))
        {
            return DeploymentPlanManagementError.MissingName;
        }
        var count = await _session.QueryIndex<DeploymentPlanIndex>(index => index.Name == name && index.DocumentId != id).CountAsync();
        return count > 0 ? DeploymentPlanManagementError.DuplicateName : DeploymentPlanManagementError.None;
    }

    private async Task SaveAsync(DeploymentPlan plan)
    {
        await _session.SaveAsync(plan);
        _deploymentPlans = null;
    }

    /// <inheritdoc />
    public DeploymentStep CloneStep(DeploymentStep step)
    {
        var clone = JsonSerializer.Deserialize<DeploymentStep>(JsonSerializer.Serialize<DeploymentStep>(step, _jsonOptions), _jsonOptions);
        // LocalizedString is immutable presentation metadata and does not round-trip all of its constructor state.
        clone.Category = step.Category;
        return clone;
    }

    /// <inheritdoc />
    public bool StepEquals(DeploymentStep left, DeploymentStep right) =>
        JsonSerializer.Serialize<DeploymentStep>(left, _jsonOptions) == JsonSerializer.Serialize<DeploymentStep>(right, _jsonOptions);

    /// <inheritdoc />
    public async Task<DeploymentStepManagementResult> AddStepsAsync(long id, IEnumerable<DeploymentStep> steps)
    {
        var plan = await GetAsync(id);
        if (plan is null) { return new() { Error = DeploymentStepManagementError.NotFound }; }
        if (steps is null) { return new() { Error = DeploymentStepManagementError.InvalidStep }; }
        var additions = steps.ToArray();
        if (additions.Distinct(ReferenceEqualityComparer.Instance).Count() != additions.Length)
        {
            return new() { Error = DeploymentStepManagementError.InvalidStep };
        }
        var ids = plan.DeploymentSteps.Select(step => step.Id).Where(value => !string.IsNullOrEmpty(value)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var assignedIds = new List<string>();
        foreach (var step in additions)
        {
            if (step is null) { return new() { Error = DeploymentStepManagementError.InvalidStep }; }
            var stepId = string.IsNullOrWhiteSpace(step.Id) ? Guid.NewGuid().ToString("n") : step.Id;
            if (!ids.Add(stepId)) { return new() { Error = DeploymentStepManagementError.InvalidStep }; }
            assignedIds.Add(stepId);
        }
        if (additions.Length == 0) { return new(); }
        for (var index = 0; index < additions.Length; index++)
        {
            additions[index].Id = assignedIds[index];
        }
        plan.DeploymentSteps.AddRange(additions);
        await SaveAsync(plan);
        return new() { Changed = true };
    }

    /// <inheritdoc />
    public async Task<DeploymentStepManagementResult> UpdateStepAsync(long id, DeploymentStep step)
    {
        var plan = await GetAsync(id);
        if (plan is null) { return new() { Error = DeploymentStepManagementError.NotFound }; }
        if (step is null || string.IsNullOrWhiteSpace(step.Id)) { return new() { Error = DeploymentStepManagementError.InvalidStep }; }
        var index = plan.DeploymentSteps.FindIndex(candidate => string.Equals(candidate.Id, step.Id, StringComparison.OrdinalIgnoreCase));
        if (index < 0) { return new() { Error = DeploymentStepManagementError.NotFound }; }
        var current = plan.DeploymentSteps[index];
        if (current.GetType() != step.GetType() || current.Name != step.Name)
        {
            return new() { Error = DeploymentStepManagementError.InvalidStep };
        }
        step.Id = current.Id;
        if (StepEquals(current, step))
        {
            return new();
        }
        plan.DeploymentSteps[index] = step;
        await SaveAsync(plan);
        return new() { Changed = true };
    }

    /// <inheritdoc />
    public async Task<DeploymentStepManagementResult> DeleteStepAsync(long id, string stepId)
    {
        if (string.IsNullOrWhiteSpace(stepId)) { return new() { Error = DeploymentStepManagementError.InvalidStep }; }
        var plan = await GetAsync(id);
        if (plan is null) { return new() { Error = DeploymentStepManagementError.NotFound }; }
        var step = plan.DeploymentSteps.FirstOrDefault(candidate => string.Equals(candidate.Id, stepId, StringComparison.OrdinalIgnoreCase));
        if (step is null) { return new() { Error = DeploymentStepManagementError.NotFound }; }
        plan.DeploymentSteps.Remove(step);
        await SaveAsync(plan);
        return new() { Changed = true };
    }

    /// <inheritdoc />
    public async Task<DeploymentStepManagementResult> MoveStepAsync(long id, int oldIndex, int newIndex)
    {
        var plan = await GetAsync(id);
        if (plan is null || oldIndex < 0 || oldIndex >= plan.DeploymentSteps.Count)
        {
            return new() { Error = DeploymentStepManagementError.NotFound };
        }
        if (newIndex < 0 || newIndex >= plan.DeploymentSteps.Count)
        {
            return new() { Error = DeploymentStepManagementError.InvalidOrder };
        }
        if (oldIndex == newIndex) { return new(); }
        var ordered = plan.DeploymentSteps.ToList();
        var step = ordered[oldIndex];
        ordered.RemoveAt(oldIndex);
        ordered.Insert(newIndex, step);
        await ApplyOrderAsync(plan, ordered);
        return new() { Changed = true };
    }

    /// <inheritdoc />
    public async Task<DeploymentStepManagementResult> ReorderStepsAsync(long id, IReadOnlyList<string> stepIds)
    {
        var plan = await GetAsync(id);
        if (plan is null) { return new() { Error = DeploymentStepManagementError.NotFound }; }
        if (stepIds is null || stepIds.Count != plan.DeploymentSteps.Count || stepIds.Any(string.IsNullOrWhiteSpace)
            || stepIds.Distinct(StringComparer.OrdinalIgnoreCase).Count() != stepIds.Count)
        {
            return new() { Error = DeploymentStepManagementError.InvalidOrder };
        }
        var ordered = new List<DeploymentStep>();
        foreach (var stepId in stepIds)
        {
            var matches = plan.DeploymentSteps.Where(step => string.Equals(step.Id, stepId, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1) { return new() { Error = DeploymentStepManagementError.InvalidOrder }; }
            ordered.Add(matches[0]);
        }
        if (ordered.SequenceEqual(plan.DeploymentSteps)) { return new(); }
        await ApplyOrderAsync(plan, ordered);
        return new() { Changed = true };
    }

    private async Task ApplyOrderAsync(DeploymentPlan plan, List<DeploymentStep> ordered)
    {
        plan.DeploymentSteps.Clear();
        plan.DeploymentSteps.AddRange(ordered);
        await SaveAsync(plan);
    }

    private async Task<Dictionary<string, DeploymentPlan>> GetDeploymentPlans()
    {
        if (_deploymentPlans == null)
        {
            var deploymentPlanQuery = _session.Query<DeploymentPlan, DeploymentPlanIndex>();
            var deploymentPlans = await deploymentPlanQuery.ListAsync();
            _deploymentPlans = deploymentPlans.ToDictionary(x => x.Name);
        }

        return _deploymentPlans;
    }

    public async Task<bool> DoesUserHavePermissionsAsync()
    {
        var user = _httpContextAccessor.HttpContext.User;

        var result = await _authorizationService.AuthorizeAsync(user, DeploymentPermissions.ManageDeploymentPlan) &&
                     await _authorizationService.AuthorizeAsync(user, DeploymentPermissions.Export);

        return result;
    }

    public async Task<bool> DoesUserHaveExportPermissionAsync()
    {
        var user = _httpContextAccessor.HttpContext.User;

        var result = await _authorizationService.AuthorizeAsync(user, DeploymentPermissions.Export);

        return result;
    }

    public async Task<IEnumerable<string>> GetAllDeploymentPlanNamesAsync()
    {
        var deploymentPlans = await GetDeploymentPlans();

        return deploymentPlans.Keys;
    }

    public async Task<IEnumerable<DeploymentPlan>> GetAllDeploymentPlansAsync()
    {
        var deploymentPlans = await GetDeploymentPlans();

        return deploymentPlans.Values;
    }

    public async Task<IEnumerable<DeploymentPlan>> GetDeploymentPlansAsync(params string[] deploymentPlanNames)
    {
        var deploymentPlans = await GetDeploymentPlans();

        return GetDeploymentPlans(deploymentPlans, deploymentPlanNames);
    }

    private static IEnumerable<DeploymentPlan> GetDeploymentPlans(Dictionary<string, DeploymentPlan> deploymentPlans, params string[] deploymentPlanNames)
    {
        foreach (var deploymentPlanName in deploymentPlanNames)
        {
            if (deploymentPlans.TryGetValue(deploymentPlanName, out var deploymentPlan))
            {
                yield return deploymentPlan;
            }
        }
    }

    private static bool ValidName(string name) => !string.IsNullOrWhiteSpace(name);

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string[]> ValidateReplacement(IReadOnlyList<DeploymentPlan> deploymentPlans)
    {
        var errors = new Dictionary<string, string[]>();
        if (deploymentPlans is null)
        {
            errors["plans"] = ["A plan array is required."];
            return errors;
        }
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var planIndex = 0; planIndex < deploymentPlans.Count; planIndex++)
        {
            var plan = deploymentPlans[planIndex];
            var prefix = $"plans[{planIndex}]";
            if (plan is null || !ValidName(plan.Name))
            {
                errors[prefix] = ["Every plan requires a nonempty name."];
                continue;
            }
            if (!names.Add(plan.Name)) { errors[prefix + ".name"] = ["Plan names must be unique within the replacement batch."]; }
            if (plan.DeploymentSteps is null)
            {
                errors[prefix + ".steps"] = ["A step collection is required."];
                continue;
            }
            var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var instances = new HashSet<DeploymentStep>(ReferenceEqualityComparer.Instance);
            for (var stepIndex = 0; stepIndex < plan.DeploymentSteps.Count; stepIndex++)
            {
                var step = plan.DeploymentSteps[stepIndex];
                var stepPrefix = prefix + $".steps[{stepIndex}]";
                if (step is null || !instances.Add(step))
                {
                    errors[stepPrefix] = ["Every step must be a distinct non-null instance."];
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(step.Id) && !identities.Add(step.Id))
                {
                    errors[stepPrefix + ".id"] = ["Step identities must be unique within a plan."];
                }
                foreach (var (field, messages) in DeploymentStepValidation.Validate(step))
                {
                    errors[stepPrefix + "." + field] = messages;
                }
            }
        }
        return errors;
    }

    /// <summary>
    /// Creates or replaces plans while preserving steps when a caller passes a tracked plan.
    /// Invalidates request-local discovery after writes so subsequent callers see the changes.
    /// </summary>
    /// <param name="deploymentPlans">The plans whose names and steps should be saved.</param>
    public async Task CreateOrUpdateDeploymentPlansAsync(IEnumerable<DeploymentPlan> deploymentPlans)
    {
        var plans = deploymentPlans?.ToArray();
        if (ValidateReplacement(plans).Count != 0)
        {
            throw new ArgumentException("The deployment plan replacement batch is invalid.", nameof(deploymentPlans));
        }
        var names = plans.Select(x => x.Name);

        var existingDeploymentPlans = (await _session.Query<DeploymentPlan, DeploymentPlanIndex>(x => x.Name.IsIn(names))
            .ListAsync())
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var deploymentPlan in plans)
        {
            foreach (var step in deploymentPlan.DeploymentSteps) { DeploymentStepValidation.Normalize(step); }
            if (existingDeploymentPlans.TryGetValue(deploymentPlan.Name, out var existingDeploymentPlan))
            {
                DeploymentStepIdentities.EnsureUnique(deploymentPlan.DeploymentSteps, existingDeploymentPlan.DeploymentSteps);
                var steps = deploymentPlan.DeploymentSteps.ToArray();
                existingDeploymentPlan.Name = deploymentPlan.Name;
                existingDeploymentPlan.DeploymentSteps.Clear();
                existingDeploymentPlan.DeploymentSteps.AddRange(steps);

                await SaveAsync(existingDeploymentPlan);
            }
            else
            {
                DeploymentStepIdentities.EnsureUnique(deploymentPlan.DeploymentSteps);
                await SaveAsync(deploymentPlan);
            }
        }

        _deploymentPlans = null;
    }
}
