using System.IO;

namespace Orchard.Environment.Extensions.Compilers
{
    public static class Files
    {
        public static bool IsNewer(string path1, string path2)
        {
            return File.Exists(path1) && (!File.Exists(path2) || File.GetLastWriteTimeUtc(path1) > File.GetLastWriteTimeUtc(path2));
        }

        public static string GetNewest(string path1, string path2)
        {
            if (Files.IsNewer(path2, path1))
            {
                return path2;
            }
            else if (File.Exists(path1))
            {
                return path1;
            }

            return null;
        }
    }
}
