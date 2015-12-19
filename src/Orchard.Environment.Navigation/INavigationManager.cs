using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Environment.Navigation
{
    public interface INavigationManager : IDependency
    {
        IEnumerable<MenuItem> BuildMenu(string name);
    }
}
