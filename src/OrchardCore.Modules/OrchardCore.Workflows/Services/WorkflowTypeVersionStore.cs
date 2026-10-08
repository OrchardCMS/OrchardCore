using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    private readonly WorkflowVersionOptions _options;
    private readonly ILogger _logger;

    // Resuming many instances of a workflow type loads the same versions again and again.
    private readonly Dictionary<string, WorkflowTypeVersion> _versions = [];

    public WorkflowTypeVersionStore(
        ISession session,
        IIdGenerator idGenerator,
        IClock clock,
        IHttpContextAccessor httpContextAccessor,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IOptions<WorkflowVersionOptions> options,
        ILogger<WorkflowTypeVersionStore> logger)
    {
        _session = session;
        _idGenerator = idGenerator;
        _clock = clock;
        _httpContextAccessor = httpContextAccessor;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<WorkflowTypeVersion> GetAsync(string versionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(versionId);

        if (!_versions.TryGetValue(versionId, out var version))
        {
            version = await _session.Query<WorkflowTypeVersion, WorkflowTypeVersionIndex>(index => index.VersionId == versionId).FirstOrDefaultAsync();

            if (version is not null)
            {
                _versions[versionId] = version;
            }
        }

        return version;
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
            IsActivity = workflowType.IsActivity,
            BranchingMode = workflowType.BranchingMode,
            FaultOnScriptErrors = workflowType.FaultOnScriptErrors,
            RecordActivityData = workflowType.RecordActivityData,
            Activities = workflowType.Activities.Select(activity => activity.Clone()).ToList(),
            Transitions = workflowType.Transitions.Select(transition => transition.Clone()).ToList(),
            Variables = workflowType.Variables.Select(variable => variable.Clone()).ToList(),
        };

        await _session.SaveAsync(version);
        workflowType.VersionId = version.VersionId;

        await PruneAsync(workflowType.WorkflowTypeId);

        return version;
    }

    /// <inheritdoc />
    public async Task<WorkflowType> GetWorkflowTypeAsync(WorkflowType workflowType, string versionId)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        if (string.IsNullOrEmpty(versionId) || versionId == workflowType.VersionId)
        {
            return workflowType;
        }

        var version = await GetAsync(versionId);

        if (version is null || version.WorkflowTypeId != workflowType.WorkflowTypeId)
        {
            _logger.LogWarning(
                "The version '{VersionId}' of the workflow type '{WorkflowTypeId}' doesn't exist; using the current definition instead.",
                versionId,
                workflowType.WorkflowTypeId);

            return workflowType;
        }

        return version.ToWorkflowType(workflowType);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string workflowTypeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowTypeId);

        foreach (var version in await ListAsync(workflowTypeId))
        {
            _versions.Remove(version.VersionId);
            _session.Delete(version);
        }
    }

    // Keeps the most recent versions, and the older ones that instances run on.
    private async Task PruneAsync(string workflowTypeId)
    {
        if (_options.MaxCount <= 0)
        {
            return;
        }

        foreach (var version in (await ListAsync(workflowTypeId)).Skip(_options.MaxCount))
        {
            var isPinned = await _session.QueryIndex<WorkflowIndex>(index => index.WorkflowTypeVersionId == version.VersionId).CountAsync() > 0;

            if (!isPinned)
            {
                _versions.Remove(version.VersionId);
                _session.Delete(version);
            }
        }
    }

    // What a version holds besides its metadata: two definitions with the same fingerprint run the same way.
    private string Fingerprint(
        bool isSingleton,
        int lockTimeout,
        int lockExpiration,
        bool deleteFinishedWorkflows,
        bool isActivity,
        WorkflowBranchingMode branchingMode,
        bool faultOnScriptErrors,
        bool recordActivityData,
        IList<ActivityRecord> activities,
        IList<Transition> transitions,
        IList<WorkflowVariableDefinition> variables)
        => JsonSerializer.Serialize(
            new
            {
                IsSingleton = isSingleton,
                LockTimeout = lockTimeout,
                LockExpiration = lockExpiration,
                DeleteFinishedWorkflows = deleteFinishedWorkflows,
                IsActivity = isActivity,
                BranchingMode = branchingMode,
                FaultOnScriptErrors = faultOnScriptErrors,
                RecordActivityData = recordActivityData,
                Activities = activities,
                Transitions = transitions,
                Variables = variables,
            },
            _jsonSerializerOptions);

    private string Fingerprint(WorkflowType workflowType)
        => Fingerprint(
            workflowType.IsSingleton,
            workflowType.LockTimeout,
            workflowType.LockExpiration,
            workflowType.DeleteFinishedWorkflows,
            workflowType.IsActivity,
            workflowType.BranchingMode,
            workflowType.FaultOnScriptErrors,
            workflowType.RecordActivityData,
            workflowType.Activities,
            workflowType.Transitions,
            workflowType.Variables);

    private string Fingerprint(WorkflowTypeVersion version)
        => Fingerprint(
            version.IsSingleton,
            version.LockTimeout,
            version.LockExpiration,
            version.DeleteFinishedWorkflows,
            version.IsActivity,
            version.BranchingMode,
            version.FaultOnScriptErrors,
            version.RecordActivityData,
            version.Activities,
            version.Transitions,
            version.Variables);
}
