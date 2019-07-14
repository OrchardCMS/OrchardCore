using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.DisplayManagement
{
    public interface IViewResultFilter
    {
        Task OnResultExecutionAsync(ActionContext context);
    }
}
