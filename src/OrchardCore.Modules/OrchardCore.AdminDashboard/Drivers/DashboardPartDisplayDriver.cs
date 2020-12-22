using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.Utilities;
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

        public override IDisplayResult Edit(DashboardPart widgetPart, BuildPartEditorContext context)
        {
            return null;
        }

        public override Task<IDisplayResult> UpdateAsync(DashboardPart part, UpdatePartEditorContext context)
        {
            return Task.FromResult(Edit(part, context));
        }
    }
}
