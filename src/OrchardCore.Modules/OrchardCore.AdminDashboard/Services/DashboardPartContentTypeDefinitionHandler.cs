using System.Text.Json.Nodes;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Modules;

namespace OrchardCore.AdminDashboard.Services;

public sealed class DashboardPartContentTypeDefinitionHandler : ContentDefinitionHandlerBase
{
    public override void ContentTypeBuilding(BuildingContentTypeContext context)
    {
        if (!context.Record.Settings.TryGetPropertyValue(nameof(ContentTypeSettings), out var node))
        {
            return;
        }

        var settings = node.ToObject<ContentTypeSettings>();

        if (settings.Stereotype == null || !string.Equals(settings.Stereotype, AdminDashboardConstants.Stereotype, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (context.Record.ContentTypePartDefinitionRecords.Any(x => x.Name.EqualsOrdinalIgnoreCase(nameof(DashboardPart))))
        {
            return;
        }

        context.Record.ContentTypePartDefinitionRecords.Add(new ContentTypePartDefinitionRecord
        {
            Name = nameof(DashboardPart),
            PartName = nameof(DashboardPart),
            Settings = new JsonObject()
            {
                [nameof(ContentSettings)] = JObject.FromObject(new ContentSettings
                {
                    IsCodeManaged = true,
                }),
            },
        });
    }

    public override void ContentTypePartBuilding(ContentTypePartContextBuilding context)
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

    public override void ContentPartDefinitionBuilding(ContentPartDefinitionContextBuilding context)
    {
        if (context.Record is not null || context.Name != nameof(DashboardPart))
        {
            return;
        }

        context.Record = new ContentPartDefinitionRecord()
        {
            Name = context.Name,
            Settings = new JsonObject()
            {
                [nameof(ContentPartSettings)] = JObject.FromObject(new ContentPartSettings
                {
                    Attachable = false,
                    Reusable = true,
                }),
                [nameof(ContentSettings)] = JObject.FromObject(new ContentSettings
                {
                    IsCodeManaged = true,
                }),
            },
        };
    }
}
