using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public class ContentPickerFieldImportHandler : StandardFieldImportHandler
{
    private Dictionary<string, IEnumerable<ContentItemIndex>> _data = new(StringComparer.OrdinalIgnoreCase);
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ISession _session;

    public ContentPickerFieldImportHandler(
        IStringLocalizer<ContentPickerFieldImportHandler> stringLocalizer,
        IContentDefinitionManager contentDefinitionManager,
        ISession session
        )
        : base(stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _session = session;
    }

    protected override async Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var settings = context.ContentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>();
        var contentTypes = GetContentTypes(settings);

        var items = await GetItemsAsync(contentTypes, text?.Trim());

        context.ContentPart.Alter<ContentPickerField>(context.ContentPartFieldDefinition.Name, (field) =>
        {
            var selectedItems = items.Select(x => x.ContentItemId).ToArray();

            if (settings.Multiple)
            {
                field.ContentItemIds = selectedItems;
            }
            else if (selectedItems.Length > 0)
            {
                field.ContentItemIds = new[] { selectedItems[0] };
            }
            else
            {
                field.ContentItemIds = Array.Empty<string>();
            }
        });
    }

    protected override async Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>();
        List<string> contentTypes = GetContentTypes(settings);

        var field = context.ContentPart.Get<ContentPickerField>(context.ContentPartFieldDefinition.Name);

        if (field?.ContentItemIds == null || field.ContentItemIds.Length == 0)
        {
            return null;
        }

        var items = await GetCachedItems(contentTypes);

        var values = items.Where(x => contentTypes.Contains(x.Key))
            .SelectMany(x => x.Value)
            .Where(x => field.ContentItemIds.Contains(x.ContentItemId))
            .Select(x => x.DisplayText);

        return string.Join("|", values);
    }

    private List<string> GetContentTypes(ContentPickerFieldSettings settings)
    {
        var contentTypes = new List<string>();

        if (settings.DisplayAllContentTypes)
        {
            contentTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Select(x => x.Name)
                .ToList();
        }
        else if (settings.DisplayedStereotypes != null && settings.DisplayedStereotypes.Length > 0)
        {
            contentTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Where(x => x.TryGetStereotype(out var stereotype) && settings.DisplayedStereotypes.Contains(stereotype))
                .Select(x => x.Name)
                .ToList();
        }
        else
        {
            contentTypes = settings.DisplayedContentTypes.ToList();
        }

        return contentTypes;
    }

    protected override string Description(ImportContentFieldContext context)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

        if (settings.Multiple)
        {
            return S["All values for {0}. Separate each value with bar (i.e., | )", context.ContentPartFieldDefinition.DisplayName()];
        }

        return S["A value for {0}", context.ContentPartFieldDefinition.DisplayName()];
    }

    protected override bool IsRequired(ImportContentFieldContext context)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

        return settings?.Required ?? false;
    }

    protected override string BindingPropertyName => nameof(ContentPickerField.ContentItemIds);


    private async Task<IEnumerable<ContentItemIndex>> GetItemsAsync(IEnumerable<string> contentTypes, string displayText)
    {
        await GetCachedItems(contentTypes);

        return _data.Where(x => contentTypes.Contains(x.Key))
            .SelectMany(x => x.Value)
            .Where(x => string.Equals(x.DisplayText, displayText, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<Dictionary<string, IEnumerable<ContentItemIndex>>> GetCachedItems(IEnumerable<string> contentTypes)
    {
        var missingItems = new List<string>();

        foreach (var contentType in contentTypes)
        {
            if (!_data.ContainsKey(contentType))
            {
                missingItems.Add(contentType);
            }
        }

        if (missingItems.Count > 0)
        {
            var records = (await _session.QueryIndex<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(missingItems)).ListAsync())
                .GroupBy(x => x.ContentType)
                .Select(x => new
                {
                    ContentType = x.Key,
                    ContentItems = x.Select(y => y)
                }).ToList();

            foreach (var record in records)
            {
                _data.TryAdd(record.ContentType, record.ContentItems);
            }
        }

        return _data;
    }
}
