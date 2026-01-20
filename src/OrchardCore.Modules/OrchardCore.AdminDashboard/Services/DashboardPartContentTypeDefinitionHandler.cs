using System.Text.Json.Nodes;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Modules;

namespace OrchardCore.AdminDashboard.Services;

public sealed class DashboardPartContentTypeDefinitionHandler : IContentDefinitionHandler
{
    /// <summary>
    /// Adds the <see cref="DashboardPart"/> to the content type definition when the stereotype is set to 'DashboardWidget'.
    /// This occurs during the content type building process, allowing the content type to function as a dashboard widget.
    /// </summary>
    public void ContentTypeBuilding(ContentTypeBuildingContext context)
    {
        if (context?.Record?.Settings is null || !context.Record.Settings.TryGetPropertyValue(nameof(ContentTypeSettings), out var node))
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
                    IsSystemDefined = true,
                }),
            },
        });
    }

    /// <summary>
    /// Marks the part on the content type as a system type to prevent its removal.
    /// This ensures that the part remains integral to the content type and cannot be deleted.
    /// </summary>
    public void ContentTypePartBuilding(ContentTypePartBuildingContext context)
    {
        if (context?.Record?.Settings is null || !context.Record.PartName.EqualsOrdinalIgnoreCase(nameof(DashboardPart)))
        {
            return;
        }

        var settings = context.Record.Settings[nameof(ContentSettings)]?.ToObject<ContentSettings>()
            ?? new ContentSettings();

        settings.IsSystemDefined = true;

        context.Record.Settings[nameof(ContentSettings)] = JObject.FromObject(settings);
    }

    /// <summary>
    /// Creates a definition if the Record is null and the part name is 'DashboardPart'.
    /// This ensures that the 'DashboardPart' has a valid definition when it is missing.
    /// </summary>
    public void ContentPartBuilding(ContentPartBuildingContext context)
    {
        if (context.Record is not null || context.PartName != nameof(DashboardPart))
        {
            return;
        }

        context.Record = new ContentPartDefinitionRecord()
        {
            Name = context.PartName,
            Settings = new JsonObject()
            {
                [nameof(ContentPartSettings)] = JObject.FromObject(new ContentPartSettings
                {
                    Attachable = false,
                    Reusable = false,
                }),
                [nameof(ContentSettings)] = JObject.FromObject(new ContentSettings
                {
                    IsSystemDefined = true,
                }),
            },
        };
    }

    public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context)
    {
    }
}
