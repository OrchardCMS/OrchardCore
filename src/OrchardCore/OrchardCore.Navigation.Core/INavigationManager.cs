using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Navigation
{
    public interface INavigationManager
    {
        IEnumerable<MenuItem> BuildMenu(string name, ActionContext context);
    }
}
