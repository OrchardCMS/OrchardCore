using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.Cms.Module.Models;
using OrchardCore.Templates.Cms.Module.Settings;
using OrchardCore.Templates.Cms.Module.ViewModels;

namespace OrchardCore.Templates.Cms.Module.Drivers
{
    public class MyTestPartDisplayDriver : ContentPartDisplayDriver<MyTestPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MyTestPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(MyTestPart part, BuildPartDisplayContext context)
        {
            return Initialize<MyTestPartViewModel>(GetDisplayShapeType(context), m => BuildViewModel(m, part, context))
                .Location("Detail", "Content:10")
                .Location("Summary", "Content:10")
                ;
        }

        public override IDisplayResult Edit(MyTestPart part, BuildPartEditorContext context)
        {
            return Initialize<MyTestPartViewModel>(GetEditorShapeType(context), model =>
            {
                model.Show = part.Show;
                model.ContentItem = part.ContentItem;
                model.MyTestPart = part;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MyTestPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Show);

            return Edit(model);
        }

        private static void BuildViewModel(MyTestPartViewModel model, MyTestPart part, BuildPartDisplayContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<MyTestPartSettings>();

            model.ContentItem = part.ContentItem;
            model.MySetting = settings.MySetting;
            model.Show = part.Show;
            model.MyTestPart = part;
            model.Settings = settings;
        }
    }
}
