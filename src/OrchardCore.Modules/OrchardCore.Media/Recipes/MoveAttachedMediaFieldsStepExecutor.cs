using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.FileStorage;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Media.Recipes;

public sealed class MoveAttachedMediaFieldsStepExecutor
{
    private const int BatchSize = 50;

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IStore _store;
    private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
    private readonly IMediaFileStore _mediaFileStore;
    private readonly ILogger _logger;

    public MoveAttachedMediaFieldsStepExecutor(
        IContentDefinitionManager contentDefinitionManager,
        IStore store,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        IMediaFileStore mediaFileStore,
        ILogger<MoveAttachedMediaFieldsStepExecutor> logger)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _store = store;
        _attachedMediaFieldFileService = attachedMediaFieldFileService;
        _mediaFileStore = mediaFileStore;
        _logger = logger;
    }

    public async Task ExecuteAsync(string[] contentTypes)
    {
        var attachedFieldsByType = await GetAttachedFieldsByTypeAsync(contentTypes);

        foreach (var (contentType, attachedFields) in attachedFieldsByType)
        {
            await ProcessContentTypeAsync(contentType, attachedFields);
        }
    }

    private async Task<Dictionary<string, AttachedMediaFieldDefinition[]>> GetAttachedFieldsByTypeAsync(string[] contentTypes)
    {
        var selectedContentTypes = contentTypes?.Length > 0
            ? new HashSet<string>(contentTypes.Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.OrdinalIgnoreCase)
            : null;

        var typeDefinitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();
        var attachedFieldsByType = new Dictionary<string, AttachedMediaFieldDefinition[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var typeDefinition in typeDefinitions)
        {
            if (selectedContentTypes is not null && !selectedContentTypes.Contains(typeDefinition.Name))
            {
                continue;
            }

            var attachedFields = typeDefinition.Parts
                .SelectMany(part => part.PartDefinition.Fields
                    .Where(field => field.FieldDefinition?.Name == nameof(MediaField)
                        && string.Equals(field.Editor(), "Attached", StringComparison.OrdinalIgnoreCase))
                    .Select(field => new AttachedMediaFieldDefinition(part.Name, field.Name)))
                .ToArray();

            if (attachedFields.Length > 0)
            {
                attachedFieldsByType[typeDefinition.Name] = attachedFields;
            }
        }

        return attachedFieldsByType;
    }

    private async Task ProcessContentTypeAsync(string contentType, AttachedMediaFieldDefinition[] attachedFields)
    {
        var documentId = 0L;

        for (var skip = 0; ; skip += BatchSize)
        {
            using var session = _store.CreateSession();

            var latestIndexes = await session.QueryIndex<ContentItemIndex>(x => x.ContentType == contentType && x.DocumentId > documentId && (x.Latest || x.Published))
                .OrderBy(x => x.DocumentId)
                .Skip(skip)
                .Take(BatchSize)
                .ListAsync();

            if (!latestIndexes.Any())
            {
                break;
            }

            var contentItemIds = latestIndexes
                .Select(x => x.ContentItemId)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            documentId = latestIndexes.Max(x => x.DocumentId);

            var versions = await session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId.IsIn(contentItemIds)).ListAsync();

            var hasChanges = false;

            foreach (var contentItemVersions in versions.GroupBy(x => x.ContentItemId, StringComparer.OrdinalIgnoreCase))
            {
                var pathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var version in contentItemVersions)
                {
                    if (await UpdateAttachedFieldsAsync(version, attachedFields, pathMap))
                    {
                        hasChanges = true;
                        await session.SaveAsync(version);
                    }
                }
            }

            if (hasChanges)
            {
                await session.SaveChangesAsync();
            }
        }
    }

    private async Task<bool> UpdateAttachedFieldsAsync(
        ContentItem contentItem,
        AttachedMediaFieldDefinition[] attachedFields,
        Dictionary<string, string> pathMap)
    {
        var changed = false;

        foreach (var attachedField in attachedFields)
        {
            var part = contentItem.Get<ContentPart>(attachedField.PartName);
            if (part is null)
            {
                continue;
            }

            var field = part.Get<MediaField>(attachedField.FieldName);
            if (field?.Paths is null || field.Paths.Length == 0)
            {
                continue;
            }

            var originalPaths = field.Paths;
            var updatedPaths = new string[originalPaths.Length];
            var existingAttachedFileNames = field.GetAttachedFileNames();
            var updatedAttachedFileNames = new string[originalPaths.Length];
            var fieldChanged = false;

            for (var i = 0; i < originalPaths.Length; i++)
            {
                var originalPath = _mediaFileStore.NormalizePath(originalPaths[i]);
                var updatedPath = await GetOrCreateAttachedPathAsync(originalPath, contentItem, pathMap);

                updatedPaths[i] = updatedPath;
                updatedAttachedFileNames[i] = GetAttachedFileName(existingAttachedFileNames, originalPaths, i);

                if (!string.Equals(originalPaths[i], updatedPath, StringComparison.OrdinalIgnoreCase))
                {
                    fieldChanged = true;
                }
            }

            if (!existingAttachedFileNames.SequenceEqual(updatedAttachedFileNames))
            {
                field.SetAttachedFileNames(updatedAttachedFileNames);
                fieldChanged = true;
            }

            if (fieldChanged)
            {
                field.Paths = updatedPaths;
                part.Apply(attachedField.FieldName, field);
                contentItem.Apply(attachedField.PartName, part);
                changed = true;
            }
        }

        return changed;
    }

    private async Task<string> GetOrCreateAttachedPathAsync(string originalPath, ContentItem contentItem, Dictionary<string, string> pathMap)
    {
        if (string.IsNullOrEmpty(originalPath))
        {
            return originalPath;
        }

        if (pathMap.TryGetValue(originalPath, out var attachedPath))
        {
            return attachedPath;
        }

        attachedPath = await _attachedMediaFieldFileService.MoveFileToContentItemFolderAsync(originalPath, contentItem);

        if (string.Equals(attachedPath, originalPath, StringComparison.OrdinalIgnoreCase)
            && await _mediaFileStore.GetFileInfoAsync(originalPath) is null)
        {
            _logger.LogWarning(
                "Skipped moving missing media file '{MediaPath}' for content item '{ContentItemId}'.",
                originalPath,
                contentItem.ContentItemId);
        }

        pathMap[originalPath] = attachedPath;

        return attachedPath;
    }

    private static string GetAttachedFileName(string[] existingAttachedFileNames, string[] originalPaths, int index)
    {
        if (existingAttachedFileNames is not null
            && index < existingAttachedFileNames.Length
            && !string.IsNullOrWhiteSpace(existingAttachedFileNames[index]))
        {
            return existingAttachedFileNames[index];
        }

        return Path.GetFileName(originalPaths[index]);
    }

    private sealed record AttachedMediaFieldDefinition(string PartName, string FieldName);
}
