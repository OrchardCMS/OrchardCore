using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services
{
    public interface ICultureStore
    {
        Task<IEnumerable<ICulture>> GetAllCultures();

        Task SaveAsync(ICulture culture, CancellationToken cancellationToken);
        
        Task DeleteAsync(ICulture culture, CancellationToken cancellationToken);
        
        Task<ICulture> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
    }
}
