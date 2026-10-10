using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Contents;
using YesSql;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Creates or updates content items from rows. Each row sets the parts and fields its mappings write, and an item is
/// validated before it is saved, as the editor would. Rows that can't be saved go to the Rejected output, with the
/// reason. The user the run is for must be allowed to edit, and to publish, the content type.
/// </summary>
public sealed class SaveContentItemsStep : DataPipelineStepType<SaveContentItemsStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "SaveContentItems";

    /// <summary>
    /// The output of the rows that couldn't be saved.
    /// </summary>
    public const string Rejected = "Rejected";

    /// <summary>
    /// The field of the rejected rows that holds the reason.
    /// </summary>
    public const string ErrorField = "Error";

    private readonly IStringLocalizer S;

    public SaveContentItemsStep(IStringLocalizer<SaveContentItemsStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Save content items"];

    public override LocalizedString Description => S["Creates or updates content items from rows."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

    public override string Icon => "fa-solid fa-file-pen";

    public override IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step)
        => [new(DataPipelinePort.Input, S["Rows"]) { IsRequired = true }];

    public override IReadOnlyList<DataPipelinePort> GetOutputs(DataPipelineStep step)
        => [new(Rejected, S["Rejected"])];

    public override async Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var input = context.GetInputFields();
        var settings = GetSettings(context.Step);

        context.SetOutputFields([.. input, new DataField(ErrorField, S["Error"], DataFieldType.Text)], Rejected);

        var columns = await GetColumnsAsync(settings, context.Services);

        if (columns is null)
        {
            context.AddError(S["Choose the content type of the items."]);

            return;
        }

        foreach (var error in Validate(settings, input, columns))
        {
            context.AddError(error);
        }
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var services = context.Services;
        var columns = await GetColumnsAsync(settings, services)
            ?? throw new InvalidOperationException(S["The content type '{0}' doesn't exist.", settings.ContentType ?? string.Empty]);

        var user = context.Run.User;
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        if (user is null || !await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.EditContent, settings.ContentType) ||
            (settings.Publish && !await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.PublishContent, settings.ContentType)))
        {
            throw new InvalidOperationException(S["The user the pipeline runs for may not edit or publish '{0}' content items.", settings.ContentType]);
        }

        var contentManager = services.GetRequiredService<IContentManager>();
        var session = services.GetRequiredService<ISession>();
        var rejected = context.GetOutput(Rejected);
        var batchSize = Math.Max(1, settings.BatchSize);
        var (created, updated, skipped, failed, pending) = (0L, 0L, 0L, 0L, 0);

        await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
        {
            var errors = Validate(settings, batch.Fields, columns);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(errors[0]);
            }

            var keyIndex = string.IsNullOrEmpty(settings.KeyField) ? -1 : batch.IndexOf(settings.KeyField);
            var mappings = settings.Mappings
                .Select(mapping => (Column: columns.First(column => column.Field.Name == mapping.Target), Index: batch.IndexOf(mapping.Source)))
                .ToArray();
            var rejectedRows = new List<object[]>();

            foreach (var row in batch.Rows)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var key = keyIndex < 0 ? null : DataValues.ToText(row[keyIndex])?.Trim();
                var outcome = await SaveAsync(contentManager, session, settings, key, row, mappings, user);

                switch (outcome.Result)
                {
                    case SaveResult.Created:
                        created++;
                        break;
                    case SaveResult.Updated:
                        updated++;
                        break;
                    case SaveResult.Skipped:
                        skipped++;
                        break;
                    default:
                        failed++;
                        rejectedRows.Add([.. row, outcome.Error]);
                        break;
                }

                if (outcome.Result is SaveResult.Created or SaveResult.Updated && ++pending >= batchSize)
                {
                    // Save what is pending, and forget it, so a large import doesn't keep every item in memory.
                    await session.SaveChangesAsync(context.CancellationToken);
                    session.DetachAll();
                    pending = 0;
                }
            }

            if (rejectedRows.Count > 0 && rejected.IsConnected)
            {
                await rejected.WriteAsync(new DataBatch(rejected.Fields.Count > 0 ? rejected.Fields : [.. batch.Fields, new DataField(ErrorField, ErrorField, DataFieldType.Text)], rejectedRows), context.CancellationToken);
            }
        }

        await session.SaveChangesAsync(context.CancellationToken);

        if (failed > 0)
        {
            context.LogWarning(S["{0} rows couldn't be saved.", failed]);
        }

        context.AddDelivery(S["Created {0} and updated {1} '{2}' content items; skipped {3} and rejected {4} rows.", created, updated, settings.ContentType, skipped, failed]);
    }

    private async Task<(SaveResult Result, string Error)> SaveAsync(
        IContentManager contentManager,
        ISession session,
        SaveContentItemsStepSettings settings,
        string key,
        object[] row,
        (ContentDataColumn Column, int Index)[] mappings,
        ClaimsPrincipal user)
    {
        var contentItemId = string.IsNullOrEmpty(key) ? null : await FindAsync(session, settings, key);

        if (contentItemId is null)
        {
            if (settings.Mode == ContentItemSaveMode.UpdateOnly)
            {
                return (SaveResult.Rejected, S["No '{0}' content item matches '{1}'.", settings.ContentType, key ?? string.Empty]);
            }

            var item = await contentManager.NewAsync(settings.ContentType);
            item.Owner = user.FindFirstValue(ClaimTypes.NameIdentifier);
            item.Author = user.Identity?.Name;

            if (settings.MatchBy == ContentItemMatch.DisplayText && !string.IsNullOrEmpty(key))
            {
                item.DisplayText = key;
            }

            Apply(item, row, mappings);

            await contentManager.UpdateAsync(item);
            var validation = await contentManager.ValidateAsync(item);

            if (!validation.Succeeded)
            {
                return (SaveResult.Rejected, string.Join(" ", validation.Errors.Select(error => error.ErrorMessage)));
            }

            await contentManager.CreateAsync(item, settings.Publish ? VersionOptions.Published : VersionOptions.Draft);

            return (SaveResult.Created, null);
        }

        if (settings.Mode == ContentItemSaveMode.CreateOnly)
        {
            return (SaveResult.Skipped, null);
        }

        var existing = await contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);

        if (existing is null || existing.ContentType != settings.ContentType)
        {
            return (SaveResult.Rejected, S["No '{0}' content item matches '{1}'.", settings.ContentType, key]);
        }

        Apply(existing, row, mappings);

        await contentManager.UpdateAsync(existing);
        var result = await contentManager.ValidateAsync(existing);

        if (!result.Succeeded)
        {
            await contentManager.DiscardDraftAsync(existing);

            return (SaveResult.Rejected, string.Join(" ", result.Errors.Select(error => error.ErrorMessage)));
        }

        if (settings.Publish)
        {
            await contentManager.PublishAsync(existing);
        }
        else
        {
            await contentManager.SaveDraftAsync(existing);
        }

        return (SaveResult.Updated, null);
    }

    private static async Task<string> FindAsync(ISession session, SaveContentItemsStepSettings settings, string key)
    {
        var contentType = settings.ContentType;

        var index = settings.MatchBy == ContentItemMatch.DisplayText
            ? await session.QueryIndex<ContentItemIndex>(index => index.ContentType == contentType && index.Latest && index.DisplayText == key).FirstOrDefaultAsync()
            : await session.QueryIndex<ContentItemIndex>(index => index.ContentType == contentType && index.Latest && index.ContentItemId == key).FirstOrDefaultAsync();

        return index?.ContentItemId;
    }

    private static void Apply(ContentItem item, object[] row, (ContentDataColumn Column, int Index)[] mappings)
    {
        foreach (var (column, index) in mappings)
        {
            column.Write(item, index < row.Length ? row[index] : null);
        }
    }

    private List<string> Validate(SaveContentItemsStepSettings settings, IReadOnlyList<DataField> input, List<ContentDataColumn> columns)
    {
        var errors = new List<string>();

        if (!string.IsNullOrEmpty(settings.KeyField) && input.IndexOf(settings.KeyField) < 0)
        {
            errors.Add(S["The input has no field named '{0}'.", settings.KeyField]);
        }

        if (string.IsNullOrEmpty(settings.KeyField) && settings.Mode != ContentItemSaveMode.CreateOnly)
        {
            errors.Add(S["Choose the field that finds the items to update."]);
        }

        if (settings.Mappings.Count == 0)
        {
            errors.Add(S["Map at least one field to the content items."]);
        }

        foreach (var mapping in settings.Mappings)
        {
            var column = columns.FirstOrDefault(column => column.Field.Name == mapping.Target);

            if (column?.Write is null)
            {
                errors.Add(S["'{0}' is not a part or field of '{1}' that can be written.", mapping.Target ?? string.Empty, settings.ContentType]);
            }

            if (input.IndexOf(mapping.Source) < 0)
            {
                errors.Add(S["The input has no field named '{0}'.", mapping.Source ?? string.Empty]);
            }
        }

        return errors;
    }

    private async Task<List<ContentDataColumn>> GetColumnsAsync(SaveContentItemsStepSettings settings, IServiceProvider services)
    {
        if (string.IsNullOrEmpty(settings.ContentType))
        {
            return null;
        }

        var definition = await services.GetRequiredService<IContentDefinitionManager>().GetTypeDefinitionAsync(settings.ContentType);

        if (definition is null)
        {
            return null;
        }

        var options = services.GetService<IOptions<ContentDataSourceOptions>>()?.Value ?? new ContentDataSourceOptions();

        return ContentDataColumns.Build(definition, options, S);
    }

    private enum SaveResult
    {
        Created,
        Updated,
        Skipped,
        Rejected,
    }
}
