using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Environment.Navigation
{
    public interface INavigationManager
    {
        IEnumerable<MenuItem> BuildMenu(string name, ActionContext context);
    }
}
