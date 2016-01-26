using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentDisplayDriver : IDisplayDriver, IDependency
    {
        Task<IDisplayResult> DisplayAsync(ContentItem contentItem);
        Task<IDisplayResult> EditAsync(ContentItem contentItem);
        Task<IDisplayResult> UpdateAsync(ContentItem contentItem, IUpdateModel updater);
    }
}
