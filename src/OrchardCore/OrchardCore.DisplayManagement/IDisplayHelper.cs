using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement
{
    public interface IDisplayHelper
    {
        Task<IHtmlContent> ShapeExecuteAsync(IShape shape);
    }
}
