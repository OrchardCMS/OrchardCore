using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.Core.Body.Model;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.Core.Body.Drivers
{
    public class BodyPartDisplay : ContentPartDisplayDriver<BodyPart>
    {
        public override IDisplayResult Display(BodyPart bodyPart, IUpdateModel updater)
        {
            return Combine(
                Shape("BodyPart", bodyPart)
                    .Location("Detail", "Content:10"),
                Shape("BodyPart_Summary", bodyPart)
                    .Location("Summary", "Content:10")
            );

        }

        public override IDisplayResult Edit(BodyPart bodyPart, IUpdateModel updater)
        {
            return Shape("BodyPart_Edit", bodyPart).Location("Content:10");
        }

        public override async Task<IDisplayResult> UpdateAsync(BodyPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, b => b.Body);

            return Edit(model, updater);
        }
    }
}
