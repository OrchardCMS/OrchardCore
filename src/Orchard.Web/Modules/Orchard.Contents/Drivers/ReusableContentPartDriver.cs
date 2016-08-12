using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.DisplayManagement.Views;

namespace Orchard.Body.Drivers
{
    public class ReusableContentPartDisplay : ContentPartDisplayDriver
    {
        public override Task<IDisplayResult> EditAsync(ContentPart part, BuildPartEditorContext context)
        {
            if (!context.TypePartDefinition.PartDefinition.Settings.ToObject<ContentPartSettings>().Reusable)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            var shape = Shape("ContentPartHeader", model =>
            {
                model.ContentPart = part;
                model.TypePartDefinition = context.TypePartDefinition;
                return Task.CompletedTask;

            }).Location("Content:" + context.TypePartDefinition.Name);

            return Task.FromResult<IDisplayResult>(shape);
        }
    }
}
