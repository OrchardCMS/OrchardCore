using System;
using System.Threading.Tasks;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.AdminDashboard.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.AdminDashboard.Drivers
{
    public class DashboardPartDisplayDriver : ContentPartDisplayDriver<DashboardPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISiteService _siteService;

        public DashboardPartDisplayDriver(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider,
            ISiteService siteService
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _siteService = siteService;
        }

        public override Task<IDisplayResult> DisplayAsync(DashboardPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        public override IDisplayResult Edit(DashboardPart dashboardPart, BuildPartEditorContext context)
        {
            return Initialize<DashboardPartViewModel>(GetEditorShapeType(context), m => BuildViewModel(m, dashboardPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(DashboardPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Position);

            return Edit(model, context);
        }

        private void BuildViewModel(DashboardPartViewModel model, DashboardPart part)
        {
            model.Position = part.Position;
            model.Width = part.Width;
            model.Height = part.Height;
            model.DashboardPart = part;
            model.ContentItem = part.ContentItem;
        }
    }
}
