using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Dashboards;
using OrchardCore.Dashboards.Models;
using OrchardCore.Data.Migration;
using YesSql;
using YesSql.Services;
using YesSql.Sql;

namespace OrchardCore.AdminDashboard;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ISession _session;

    public Migrations(
        IContentDefinitionManager contentDefinitionManager,
        ISession session)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _session = session;
    }

    public static int Create()
    {
        // The widgets, their index and the built-in widget types are created by the Dashboards feature.
        // Shortcut other migration steps on new content definition schemas.
        return 5;
    }

    public static int UpdateFrom1()
    {
        // The built-in widget types this step created are created by the Dashboards feature, migrated first.
        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<DashboardPartIndex>(table => table
            .CreateIndex("IDX_DashboardPart_DocumentId",
                "DocumentId",
                "Position")
        );

        return 3;
    }

    public async Task<int> UpdateFrom3Async()
    {
        await _contentDefinitionManager.DeletePartDefinitionAsync("DashboardPart");

        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom4Async()
    {
        // The widgets are now on the dashboards of the Dashboards feature, they all belong to the admin dashboard.
        // Saving them adds them to the index of the dashboard widgets, that replaces the 'DashboardPartIndex'.
        var widgetTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(type => type.StereotypeEquals(DashboardConstants.WidgetStereotype))
            .Select(type => type.Name)
            .ToArray();

        if (widgetTypes.Length > 0)
        {
            var widgets = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType.IsIn(widgetTypes) && (index.Latest || index.Published))
                .ListAsync();

            foreach (var widget in widgets)
            {
                if (widget.TryGet<DashboardPart>(out var part) && string.IsNullOrEmpty(part.DashboardType))
                {
                    part.DashboardType = AdminDashboardConstants.DashboardType;
                    widget.Apply(part);
                }

                await _session.SaveAsync(widget);
            }
        }

        await SchemaBuilder.DropMapIndexTableAsync<DashboardPartIndex>();

        return 5;
    }

    // The index of the widgets before they moved to the Dashboards feature, only kept to upgrade its table.
    private sealed class DashboardPartIndex : YesSql.Indexes.MapIndex
    {
    }
}
