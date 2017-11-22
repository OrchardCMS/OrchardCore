using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Layout
{
    public interface ILayoutAccessor
    {
        Task<IShape> GetLayoutAsync();
    }
}
