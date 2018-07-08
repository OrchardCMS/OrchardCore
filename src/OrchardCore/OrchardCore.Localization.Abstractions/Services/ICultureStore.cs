using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services
{
    public interface ICultureStore
    {
        Task<CultureRecord> GetCultureRecordAsync();

        Task SaveAsync(string culture, CancellationToken cancellationToken);
        
        Task DeleteAsync(string culture, CancellationToken cancellationToken);
    }
}
