using System.Security.Claims;
using System.Text.Json;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.Entities;
using OrchardCore.Modules;
using YesSql;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Creates pipelines, saves their drafts, publishes them as versions, and deletes them.
/// </summary>
public sealed class DataPipelineManager
{
    private readonly ISession _session;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public DataPipelineManager(ISession session, IIdGenerator idGenerator, IClock clock)
    {
        _session = session;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    /// <summary>
    /// Finds a pipeline.
    /// </summary>
    /// <param name="pipelineId">The identifier of the pipeline.</param>
    /// <returns>The pipeline, or <see langword="null"/>.</returns>
    public async Task<DataPipeline> GetAsync(string pipelineId)
    {
        if (string.IsNullOrEmpty(pipelineId))
        {
            return null;
        }

        return await _session.Query<DataPipeline, DataPipelineIndex>(index => index.PipelineId == pipelineId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Lists the pipelines, ordered by name.
    /// </summary>
    /// <returns>The pipelines.</returns>
    public async Task<IReadOnlyList<DataPipeline>> ListAsync()
        => (await _session.Query<DataPipeline, DataPipelineIndex>().OrderBy(index => index.Name).ListAsync()).ToList();

    /// <summary>
    /// Creates a pipeline, with an empty draft.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <param name="description">The description of the pipeline.</param>
    /// <param name="user">The user who creates it.</param>
    /// <param name="pipelineId">The identifier of the pipeline, such as the one it has on the site it was exported from, or
    /// <see langword="null"/> for a new identifier.</param>
    /// <returns>The pipeline.</returns>
    public async Task<DataPipeline> CreateAsync(string name, string description, ClaimsPrincipal user, string pipelineId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var now = _clock.UtcNow;
        var pipeline = new DataPipeline
        {
            PipelineId = string.IsNullOrWhiteSpace(pipelineId) ? _idGenerator.GenerateUniqueId() : pipelineId.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedUtc = now,
            CreatedBy = user?.Identity?.Name,
            ModifiedUtc = now,
            Draft = new DataPipelineDefinition(),
            DraftModifiedUtc = now,
            DraftModifiedBy = user?.Identity?.Name,
            DraftModifiedByUserId = GetUserId(user),
        };

        await _session.SaveAsync(pipeline);

        return pipeline;
    }

    /// <summary>
    /// Saves the draft of a pipeline, when <paramref name="expectedRevision"/> is its current revision.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <param name="expectedRevision">The revision the change was made on.</param>
    /// <param name="change">Changes the draft.</param>
    /// <param name="user">The user who saves.</param>
    /// <returns>The new revision.</returns>
    /// <exception cref="DataPipelineConflictException">The draft was saved since the given revision.</exception>
    public async Task<int> SaveDraftAsync(DataPipeline pipeline, int expectedRevision, Action<DataPipelineDefinition> change, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(change);

        if (expectedRevision != pipeline.Revision)
        {
            throw new DataPipelineConflictException(pipeline);
        }

        var draft = Clone(pipeline.GetEditableDefinition());
        change(draft);

        pipeline.Draft = draft;
        pipeline.Revision++;
        pipeline.DraftModifiedUtc = _clock.UtcNow;
        pipeline.DraftModifiedBy = user?.Identity?.Name;
        pipeline.DraftModifiedByUserId = GetUserId(user);
        pipeline.ModifiedUtc = _clock.UtcNow;

        await _session.SaveAsync(pipeline);

        return pipeline.Revision;
    }

    /// <summary>
    /// Saves the name and description of a pipeline. They are not versioned.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <returns>A task that completes when the pipeline is saved.</returns>
    public async Task UpdateSettingsAsync(DataPipeline pipeline, string name, string description)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        pipeline.Name = name.Trim();
        pipeline.Description = description?.Trim();
        pipeline.ModifiedUtc = _clock.UtcNow;

        await _session.SaveAsync(pipeline);
    }

    /// <summary>
    /// Publishes the draft of a pipeline. A new version is created when the draft differs from the published
    /// definition. From then on, runs read and write data with the access of the user who published.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <param name="user">The user who publishes.</param>
    /// <returns>The published version.</returns>
    public async Task<DataPipelineVersion> PublishAsync(DataPipeline pipeline, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        var definition = pipeline.GetEditableDefinition();
        var now = _clock.UtcNow;
        var userId = GetUserId(user);
        DataPipelineVersion version = null;

        if (pipeline.Published is null || !AreEqual(pipeline.Published, definition) || pipeline.PublishedByUserId != userId)
        {
            version = new DataPipelineVersion
            {
                VersionId = _idGenerator.GenerateUniqueId(),
                PipelineId = pipeline.PipelineId,
                Number = pipeline.PublishedVersionNumber + 1,
                Name = pipeline.Name,
                Definition = Clone(definition),
                PublishedUtc = now,
                PublishedBy = user?.Identity?.Name,
                PublishedByUserId = userId,
            };

            await _session.SaveAsync(version);

            pipeline.Published = Clone(definition);
            pipeline.PublishedVersionId = version.VersionId;
            pipeline.PublishedVersionNumber = version.Number;
            pipeline.PublishedByUserId = userId;
            pipeline.PublishedBy = user?.Identity?.Name;
        }
        else
        {
            version = await GetVersionAsync(pipeline.PipelineId, pipeline.PublishedVersionId);
        }

        pipeline.PublishedUtc = now;
        pipeline.Draft = null;
        pipeline.Revision++;
        pipeline.ModifiedUtc = now;

        await _session.SaveAsync(pipeline);

        return version;
    }

    /// <summary>
    /// Discards the draft of a pipeline, so the designer shows the published definition again.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <returns>A task that completes when the pipeline is saved.</returns>
    public async Task DiscardDraftAsync(DataPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        if (pipeline.Published is null)
        {
            pipeline.Draft = new DataPipelineDefinition();
        }
        else
        {
            pipeline.Draft = null;
        }

        pipeline.Revision++;
        pipeline.ModifiedUtc = _clock.UtcNow;

        await _session.SaveAsync(pipeline);
    }

    /// <summary>
    /// Lists the versions of a pipeline, the most recent first.
    /// </summary>
    /// <param name="pipelineId">The identifier of the pipeline.</param>
    /// <returns>The versions.</returns>
    public async Task<IReadOnlyList<DataPipelineVersion>> ListVersionsAsync(string pipelineId)
        => (await _session.Query<DataPipelineVersion, DataPipelineVersionIndex>(index => index.PipelineId == pipelineId)
            .OrderByDescending(index => index.Number)
            .ListAsync()).ToList();

    /// <summary>
    /// Finds a version of a pipeline.
    /// </summary>
    /// <param name="pipelineId">The identifier of the pipeline.</param>
    /// <param name="versionId">The identifier of the version.</param>
    /// <returns>The version, or <see langword="null"/>.</returns>
    public async Task<DataPipelineVersion> GetVersionAsync(string pipelineId, string versionId)
    {
        if (string.IsNullOrEmpty(versionId))
        {
            return null;
        }

        return await _session.Query<DataPipelineVersion, DataPipelineVersionIndex>(index => index.PipelineId == pipelineId && index.VersionId == versionId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Copies a version into the draft.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <param name="version">The version.</param>
    /// <param name="expectedRevision">The revision the restore was asked on.</param>
    /// <param name="user">The user who restores.</param>
    /// <returns>The new revision.</returns>
    public Task<int> RestoreAsync(DataPipeline pipeline, DataPipelineVersion version, int expectedRevision, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(version);

        return SaveDraftAsync(pipeline, expectedRevision, draft =>
        {
            var copy = Clone(version.Definition);
            draft.Steps = copy.Steps;
            draft.Connections = copy.Connections;
        }, user);
    }

    /// <summary>
    /// Deletes a pipeline, its versions and its runs.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <returns>A task that completes when everything is deleted.</returns>
    public async Task DeleteAsync(DataPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        var pipelineId = pipeline.PipelineId;

        foreach (var version in await _session.Query<DataPipelineVersion, DataPipelineVersionIndex>(index => index.PipelineId == pipelineId).ListAsync())
        {
            _session.Delete(version);
        }

        foreach (var run in await _session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.PipelineId == pipelineId).ListAsync())
        {
            _session.Delete(run);
        }

        _session.Delete(pipeline);
    }

    /// <summary>
    /// Makes a deep copy of a definition.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <returns>The copy.</returns>
    public static DataPipelineDefinition Clone(DataPipelineDefinition definition)
        => JsonSerializer.Deserialize<DataPipelineDefinition>(JsonSerializer.Serialize(definition ?? new DataPipelineDefinition(), JOptions.Default), JOptions.Default);

    /// <summary>
    /// Tells whether two definitions are the same.
    /// </summary>
    /// <param name="left">A definition.</param>
    /// <param name="right">Another definition.</param>
    /// <returns><see langword="true"/> when they are the same.</returns>
    public static bool AreEqual(DataPipelineDefinition left, DataPipelineDefinition right)
        => JsonSerializer.Serialize(left, JOptions.Default) == JsonSerializer.Serialize(right, JOptions.Default);

    private static string GetUserId(ClaimsPrincipal user)
        => user?.FindFirstValue(ClaimTypes.NameIdentifier);
}
