using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Dashboards.Handlers;
using OrchardCore.Dashboards.Models;
using static OrchardCore.Tests.Modules.OrchardCore.Dashboards.DashboardTestHelper;

namespace OrchardCore.Tests.Modules.OrchardCore.Dashboards;

public class DashboardPartHandlerTests
{
    [Fact]
    public async Task ImportingAsync_WidgetWithoutDashboard_AssignsDefaultType()
    {
        var handler = new DashboardPartHandler(Options.Create(CreateOptions()));
        var widget = CreateWidget(dashboardType: null, ownerId: null);
        var part = widget.GetOrCreate<DashboardPart>();

        await handler.ImportingAsync(new ImportContentContext(widget), part);

        Assert.Equal(SiteType, widget.GetOrCreate<DashboardPart>().DashboardType);
    }

    [Fact]
    public async Task CreatingAsync_WidgetWithDashboard_KeepsItsType()
    {
        var handler = new DashboardPartHandler(Options.Create(CreateOptions()));
        var widget = CreateWidget(UserType, "user1");
        var part = widget.GetOrCreate<DashboardPart>();

        await handler.CreatingAsync(new CreateContentContext(widget), part);

        Assert.Equal(UserType, widget.GetOrCreate<DashboardPart>().DashboardType);
    }
}
