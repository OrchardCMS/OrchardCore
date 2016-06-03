using Microsoft.AspNetCore.Routing;
using Orchard.DependencyInjection;
using System;

namespace Orchard.Environment.Shell
{
    public interface IRunningShellRouterTable
    {
        IRouter GetOrAdd(string shellName, Func<string, IRouter> router);
        void Remove(string shellName);
        void Update(string shellName, IRouter router);
    }
}