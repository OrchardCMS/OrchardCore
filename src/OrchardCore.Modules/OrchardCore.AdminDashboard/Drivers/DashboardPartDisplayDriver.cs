using System.Threading.Tasks;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.AdminDashboard.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AdminDashboard.Drivers
{
    public class DashboardPartDisplayDriver : ContentPartDisplayDriver<DashboardPart>
    {
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
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Position, t => t.Width, t => t.Height);
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
