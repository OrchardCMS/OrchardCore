using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.MetaTags.Models;
using OrchardCore.MetaTags.ViewModels;
using OrchardCore.ResourceManagement;

namespace OrchardCore.MetaTags.Drivers
{
    public class MetaTagsPartDisplayDriver : ContentPartDisplayDriver<MetaTagsPart>
    {
        private readonly IResourceManager _resourceManager;

        public MetaTagsPartDisplayDriver(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override async Task<IDisplayResult> DisplayAsync(MetaTagsPart part, BuildPartDisplayContext context)
        {
            if (context.DisplayType == "Detail")
            {

                if (!string.IsNullOrWhiteSpace(part.Description))
                {
                    _resourceManager.RegisterMeta(new MetaEntry { Name = "description", Content = part.Description });
                }

                if (!string.IsNullOrWhiteSpace(part.Keywords))
                {
                    _resourceManager.RegisterMeta(new MetaEntry { Name = "keywords", Content = part.Keywords });
                }
            }

            return await base.DisplayAsync(part, context);
        }

        public override IDisplayResult Edit(MetaTagsPart seoPart)
        {
            return Initialize<MetaTagsPartViewModel>("MetaTagsPart_Edit", model =>
            {
                model.Description = seoPart.Description;
                model.Keywords = seoPart.Keywords;
            }).Location("Parts:20");
        }

        public override async Task<IDisplayResult> UpdateAsync(MetaTagsPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Description, t => t.Keywords);

            return Edit(model);
        }
    }
}
