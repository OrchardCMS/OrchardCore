using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Content.Controllers;

namespace OrchardCore.ContentFields.Services
{
    public interface IContentPickerResultProvider
    {
        Task<IEnumerable<ContentPickerResult>> GetContentItems(string searchTerm, IEnumerable<string> contentTypes);
    }
}
