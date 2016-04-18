using Orchard.DependencyInjection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Orchard.Environment.Navigation
{
    public interface INavigationManager : IDependency
    {
        IEnumerable<MenuItem> BuildMenu(string name, ActionContext context);
    }
}
