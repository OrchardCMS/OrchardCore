using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Layout
{
    public interface ILayoutAccessor
    {
        Task<IZoneHolding> GetLayoutAsync();
    }
}
