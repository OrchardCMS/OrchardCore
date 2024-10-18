using System.Text.Json.Nodes;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Modules;

namespace OrchardCore.AdminDashboard.Services;

public sealed class DashboardPartContentTypeDefinitionHandler : IContentDefinitionHandler
{
    public void TypeLoaded(LoadedContentTypeContext context)
    {
        if (!context.Record.Settings.TryGetPropertyValue(nameof(ContentTypeSettings), out var node))
        {
            return;
        }

        var settings = node.ToObject<ContentTypeSettings>();

        if (settings.Stereotype == null || !string.Equals(settings.Stereotype, "DashboardWidget", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (context.Record.ContentTypePartDefinitionRecords.Any(x => x.Name.EqualsOrdinalIgnoreCase(nameof(DashboardPart))))
        {
            return;
        }

        var pastSettings = new JsonObject()
        {
            [nameof(ContentSettings)] = JObject.FromObject(new ContentSettings
            {
                IsCodeManaged = true,
            }),
        };

        pastSettings[nameof(ContentSettings)] = JObject.FromObject(new ContentSettings
        {
            IsCodeManaged = true,
        });

        context.Record.ContentTypePartDefinitionRecords.Add(new ContentTypePartDefinitionRecord
        {
            Name = nameof(DashboardPart),
            PartName = nameof(DashboardPart),
            Settings = pastSettings,
        });
    }

    public void PartFieldLoaded(LoadedContentPartFieldContext context)
    {
    }

    public void TypePartLoaded(LoadedContentTypePartContext context)
    {
        if (!context.Record.PartName.EqualsOrdinalIgnoreCase(nameof(DashboardPart)))
        {
            return;
        }

        var settings = context.Record.Settings[nameof(ContentSettings)]?.ToObject<ContentSettings>()
            ?? new ContentSettings();

        settings.IsCodeManaged = true;

        context.Record.Settings[nameof(ContentSettings)] = JObject.FromObject(settings);
    }
}
