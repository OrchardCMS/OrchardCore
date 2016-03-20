using System.Collections.Generic;
using Orchard.DisplayManagement.Shapes;
using Orchard.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.AspNet.Html;

namespace Orchard.DisplayManagement
{
    public interface IShapeDisplay : IDependency
    {
        Task<IHtmlContent> DisplayAsync(Shape shape);
        Task<IHtmlContent> DisplayAsync(object shape);
        Task<IHtmlContent> DisplayAsync(IEnumerable<object> shapes);
    }
}