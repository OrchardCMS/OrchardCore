using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services
{
    public interface ICultureStore
    {
        Task<IEnumerable<CultureRecord>> GetAllCultures();

        Task SaveAsync(CultureRecord culture, CancellationToken cancellationToken);
        
        Task DeleteAsync(CultureRecord culture, CancellationToken cancellationToken);
        
        Task<CultureRecord> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
    }
}
