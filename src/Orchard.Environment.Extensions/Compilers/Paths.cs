using System.IO;
using Microsoft.DotNet.Tools.Common;

namespace Orchard.Environment.Extensions.Compilers
{
    public static class Paths
    {
        public static string GetFolderName(string path)
        {
            return PathUtility.GetDirectoryName(path);
        }

        public static string GetParentFolderPath(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public static string GetParentFolderName(string path)
        {
            return PathUtility.GetDirectoryName(Path.GetDirectoryName(path));
        }
    }
}
