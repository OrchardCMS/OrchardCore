using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentTypes.Services
{
    public interface IStereotypesProvider
    {
        Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync();
    }

    public class StereotypeDescription
    {
        public string Stereotype { get; set; }
        public string DisplayName { get; set; }
    }
}
