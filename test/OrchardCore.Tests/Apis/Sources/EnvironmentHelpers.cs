using System;
using System.IO;

namespace OrchardCore.Tests.Apis.Sources
{
    public static class EnvironmentHelpers
    {
        public static string GetApplicationPath()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "OrchardCore.sln")))
                {
                    break;
                }
                current = current.Parent;
            }

            if (current == null)
            {
                throw new InvalidOperationException("Could not find the solution directory");
            }

            return Path.GetFullPath(Path.Combine(current.FullName, "src", "OrchardCore.Cms.Web"));
        }
    }
}
