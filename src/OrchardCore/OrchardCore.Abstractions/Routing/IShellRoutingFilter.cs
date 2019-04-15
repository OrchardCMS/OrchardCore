using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Routing
{
    public interface IShellRoutingFilter
    {
        Task OnRoutingAsync(HttpContext httpContext);
    }
}