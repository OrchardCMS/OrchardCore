using Microsoft.AspNetCore.Routing;
using Orchard.Environment.Shell;
using System;
using System.Collections.Concurrent;

namespace Orchard.Hosting.Routing.Routes
{
    public class DefaultRunningShellRouterTable : IRunningShellRouterTable
    {
        private readonly ConcurrentDictionary<string, IRouter> _routers = new ConcurrentDictionary<string, IRouter>();

        public IRouter GetOrAdd(string shellName, Func<string, IRouter> router)
        {
            return _routers.GetOrAdd(shellName, router);
        }

        public void Remove(string shellName)
        {
            IRouter outRouter;
            _routers.TryRemove(shellName, out outRouter);
        }

        public void Update(string shellName, IRouter router)
        {
            _routers.TryUpdate(shellName, router, router);
        }
    }
}
