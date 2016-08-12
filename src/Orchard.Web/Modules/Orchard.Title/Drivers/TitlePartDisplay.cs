using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Title.Model;
using Orchard.Title.ViewModels;

namespace Orchard.Title.Drivers
{
    public class TitlePartDisplay : ContentPartDisplayDriver<TitlePart>
    {
        public override IDisplayResult Display(TitlePart titlePart)
        {
            return Combine(
                Shape("TitlePart", titlePart)
                    .Location("Detail", "Content:5"),
                Shape("TitlePart_Summary", titlePart)
                    .Location("Summary", "Content:5")
            );
        }

        public override IDisplayResult Edit(TitlePart titlePart)
        {
            return Shape<TitlePartViewModel>("TitlePart_Edit", model =>
            {
                model.Title = titlePart.Title;
                model.TitlePart = titlePart;

                return Task.CompletedTask;
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(TitlePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Title);

            return Edit(model);
        }
    }
}
