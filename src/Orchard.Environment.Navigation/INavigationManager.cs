using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Orchard.Environment.Navigation
{
    public interface INavigationManager
    {
        IEnumerable<MenuItem> BuildMenu(string name, ActionContext context);
    }
}
