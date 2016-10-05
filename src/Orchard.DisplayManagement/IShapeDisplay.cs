using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement
{
    public interface IShapeDisplay
    {
        Task<IHtmlContent> DisplayAsync(Shape shape);
        Task<IHtmlContent> DisplayAsync(object shape);
        Task<IHtmlContent> DisplayAsync(IEnumerable<object> shapes);
    }
}