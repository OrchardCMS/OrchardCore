using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Navigation
{
    public interface INavigationManager
    {
        Task<IEnumerable<MenuItem>> BuildMenuAsync(string name, ActionContext context);
    }
}
