using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Environment.Navigation
{
    public interface INavigationManager
    {
        Task<IEnumerable<MenuItem>> BuildMenuAsync(string name, ActionContext context);
    }
}
