using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.Core.Title.Model;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.Core.Title.Drivers
{
    public class TitlePartDisplay : ContentPartDisplayDriver<TitlePart>
    {
        public override IDisplayResult Display(TitlePart titlePart)
        {
            return Shape("TitlePart", titlePart)
                .Location("Detail", "Content")
                .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(TitlePart titlePart)
        {
            return Shape("TitlePart_Edit", titlePart).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(TitlePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, typeof(TitlePart), "");

            return Edit(model);
        }
    }
}
