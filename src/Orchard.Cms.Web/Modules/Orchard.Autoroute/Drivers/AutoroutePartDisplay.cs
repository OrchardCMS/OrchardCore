using System.Threading.Tasks;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Autoroute.Drivers
{
    public class AutoroutePartDisplay : ContentPartDisplayDriver<AutoroutePart>
    {
        public override IDisplayResult Edit(AutoroutePart autoroutePart)
        {
            return Shape<AutoroutePartViewModel>("AutoroutePart_Edit", model =>
            {
                model.Path = autoroutePart.Path;
                model.AutoroutePart = autoroutePart;

                return Task.CompletedTask;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(AutoroutePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Path);

            return Edit(model);
        }
    }
}
