using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.DisplayManagement
{
    public interface IAsyncViewResultFilter : IAsyncResultFilter
    {
        Task OnResultExecutionAsync(ActionContext context);
    }
}
