using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;
using ISession = YesSql.ISession;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Stores <see cref="WorkflowTypeVersion"/> documents with YesSql.
/// </summary>
public sealed class WorkflowTypeVersionStore : IWorkflowTypeVersionStore
{
    private readonly ISession _session;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public WorkflowTypeVersionStore(
        ISession session,
        IIdGenerator idGenerator,
        IClock clock,
        IHttpContextAccessor httpContextAccessor,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _session = session;
        _idGenerator = idGenerator;
        _clock = clock;
        _httpContextAccessor = httpContextAccessor;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    /// <inheritdoc />
    public Task<WorkflowTypeVersion> GetAsync(string versionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(versionId);

        return _session.Query<WorkflowTypeVersion, WorkflowTypeVersionIndex>(index => index.VersionId == versionId).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkflowTypeVersion>> ListAsync(string workflowTypeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowTypeId);

        return await _session.Query<WorkflowTypeVersion, WorkflowTypeVersionIndex>(index => index.WorkflowTypeId == workflowTypeId)
            .OrderByDescending(index => index.Version)
            .ListAsync();
    }

    /// <inheritdoc />
    public Task<WorkflowTypeVersion> GetLatestAsync(string workflowTypeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowTypeId);

        return _session.Query<WorkflowTypeVersion, WorkflowTypeVersionIndex>(index => index.WorkflowTypeId == workflowTypeId)
            .OrderByDescending(index => index.Version)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<WorkflowTypeVersion> CreateIfChangedAsync(WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(workflowType);
        ArgumentException.ThrowIfNullOrEmpty(workflowType.WorkflowTypeId);

        var latest = await GetLatestAsync(workflowType.WorkflowTypeId);

        if (latest is not null && Fingerprint(latest) == Fingerprint(workflowType))
        {
            workflowType.VersionId = latest.VersionId;

            return latest;
        }

        var user = _httpContextAccessor.HttpContext?.User;

        var version = new WorkflowTypeVersion
        {
            WorkflowTypeId = workflowType.WorkflowTypeId,
            VersionId = _idGenerator.GenerateUniqueId(),
            Version = (latest?.Version ?? 0) + 1,
            CreatedUtc = _clock.UtcNow,
            CreatedByUserId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
            CreatedByUserName = user?.Identity?.Name,
            Name = workflowType.Name,
            IsSingleton = workflowType.IsSingleton,
            LockTimeout = workflowType.LockTimeout,
            LockExpiration = workflowType.LockExpiration,
            DeleteFinishedWorkflows = workflowType.DeleteFinishedWorkflows,
            Activities = workflowType.Activities.Select(activity => activity.Clone()).ToList(),
            Transitions = workflowType.Transitions.Select(transition => transition.Clone()).ToList(),
        };

        await _session.SaveAsync(version);
        workflowType.VersionId = version.VersionId;

        return version;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string workflowTypeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowTypeId);

        foreach (var version in await ListAsync(workflowTypeId))
        {
            _session.Delete(version);
        }
    }

    // What a version holds besides its metadata: two definitions with the same fingerprint run the same way.
    private string Fingerprint(
        bool isSingleton,
        int lockTimeout,
        int lockExpiration,
        bool deleteFinishedWorkflows,
        IList<ActivityRecord> activities,
        IList<Transition> transitions)
        => JsonSerializer.Serialize(
            new
            {
                IsSingleton = isSingleton,
                LockTimeout = lockTimeout,
                LockExpiration = lockExpiration,
                DeleteFinishedWorkflows = deleteFinishedWorkflows,
                Activities = activities,
                Transitions = transitions,
            },
            _jsonSerializerOptions);

    private string Fingerprint(WorkflowType workflowType)
        => Fingerprint(
            workflowType.IsSingleton,
            workflowType.LockTimeout,
            workflowType.LockExpiration,
            workflowType.DeleteFinishedWorkflows,
            workflowType.Activities,
            workflowType.Transitions);

    private string Fingerprint(WorkflowTypeVersion version)
        => Fingerprint(
            version.IsSingleton,
            version.LockTimeout,
            version.LockExpiration,
            version.DeleteFinishedWorkflows,
            version.Activities,
            version.Transitions);
}
