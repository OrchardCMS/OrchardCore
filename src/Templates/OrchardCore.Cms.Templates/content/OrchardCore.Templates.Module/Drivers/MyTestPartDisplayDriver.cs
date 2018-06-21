using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.Module.Models;
using OrchardCore.Templates.Module.Settings;
using OrchardCore.Templates.Module.ViewModels;

namespace OrchardCore.Templates.Module.Drivers
{
    public class MyTestPartDisplayDriver : ContentPartDisplayDriver<MyTestPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MyTestPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(MyTestPart MyTestPart)
        {
            return Combine(
                Initialize<MyTestPartViewModel>("MyTestPart", m => BuildViewModel(m, MyTestPart))
                    .Location("Detail", "Content:20"),
                Initialize<MyTestPartViewModel>("MyTestPart_Summary", m => BuildViewModel(m, MyTestPart))
                    .Location("Summary", "Meta:5")
            );
        }
        
        public override IDisplayResult Edit(MyTestPart MyTestPart)
        {
            return Initialize<MyTestPartViewModel>("MyTestPart_Edit", m => BuildViewModel(m, MyTestPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(MyTestPart model, IUpdateModel updater)
        {
            var settings = GetMyTestPartSettings(model);

            await updater.TryUpdateModelAsync(model, Prefix, t => t.Show);
            
            return Edit(model);
        }

        public MyTestPartSettings GetMyTestPartSettings(MyTestPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(MyTestPart));
            var settings = contentTypePartDefinition.GetSettings<MyTestPartSettings>();

            return settings;
        }

        private Task BuildViewModel(MyTestPartViewModel model, MyTestPart part)
        {
            var settings = GetMyTestPartSettings(part);

            model.ContentItem = part.ContentItem;
            model.MySetting = settings.MySetting;
            model.Show = part.Show;
            model.MyTestPart = part;
            model.Settings = settings;

            return Task.CompletedTask;
        }
    }
}
