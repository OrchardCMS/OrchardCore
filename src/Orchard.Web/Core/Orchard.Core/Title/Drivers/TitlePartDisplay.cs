using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.Core.Title.Model;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.Core.Title.Drivers
{
    public class TitlePartDisplay : ContentPartDisplayDriver<TitlePart>
    {
        public override IDisplayResult Display(TitlePart titlePart, IUpdateModel updater)
        {
            return Combine(
                Shape("TitlePart", titlePart)
                    .Location("Detail", "Content:5"),
                Shape("TitlePart_Summary", titlePart)
                    .Location("Summary", "Content:5")
            );
        }

        public override IDisplayResult Edit(TitlePart titlePart, IUpdateModel updater)
        {
            return Shape("TitlePart_Edit", titlePart).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(TitlePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Title);

            return Edit(model, updater);
        }
    }
}
