using System;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Scope
{
    public static class ShellScopeExtensions
    {
        public static async Task UsingAsync(this ShellScope scope, Func<ShellScope, Task> execute)
        {
            if (scope == null)
            {
                await execute(ShellScope.Current);
                return;
            }

            using (scope)
            {
                await scope.ExecuteAsync(execute);
            }
        }
    }
}
