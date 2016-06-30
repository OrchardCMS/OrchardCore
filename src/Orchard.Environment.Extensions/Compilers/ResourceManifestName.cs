using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.DotNet.ProjectModel.Resources;

namespace Orchard.Environment.Extensions.Compilers
{
    internal static class ResourceManifestName
    {
        public static string CreateManifestName(string fileName, string rootNamespace)
        {
            var name = new StringBuilder();

            // Differences from the msbuild task:
            // - we do not include the name of the first class (if any) for binary resources or source code
            // - culture info is ignored

            if (rootNamespace != null && rootNamespace.Length > 0)
            {
                name.Append(rootNamespace).Append(".");
            }

            // Replace spaces in the directory name with underscores.
            // Note that spaces in the file name itself are preserved.
            var path = MakeValidIdentifier(Path.GetDirectoryName(fileName));

            // This is different from the msbuild task: we always append extensions because otherwise,
            // the emitted resource doesn't have an extension and it is not the same as in the classic
            // C# assembly
            if (ResourceUtility.IsResourceFile(fileName))
            {
                name.Append(Path.Combine(path, Path.GetFileNameWithoutExtension(fileName)));
                name.Append(".resources");
                name.Replace(Path.DirectorySeparatorChar, '.');
                name.Replace(Path.AltDirectorySeparatorChar, '.');
            }
            else
            {
                name.Append(Path.Combine(path, Path.GetFileName(fileName)));
                name.Replace(Path.DirectorySeparatorChar, '.');
                name.Replace(Path.AltDirectorySeparatorChar, '.');
            }

            return name.ToString();
        }

        /// <summary>
        /// This method is provided for compatibility with MsBuild which used to convert parts of resource names into
        /// valid identifiers
        /// </summary>
        private static string MakeValidIdentifier(string name)
        {
            var id = new StringBuilder(name.Length);

            // split the name into folder names
            var subNames = name.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            // convert every folder name
            id.Append(MakeValidFolderIdentifier(subNames[0]));

            for (int i = 1; i < subNames.Length; i++)
            {
                id.Append('.');
                id.Append(MakeValidFolderIdentifier(subNames[i]));
            }

            return id.ToString();
        }

        /// <summary>
        /// Make a folder name into an identifier
        /// </summary>
        private static string MakeValidFolderIdentifier(string name)
        {
            // give string length to avoid reallocations; +1 since the resulting string may be one char longer than the
            // original - if the name is a single underscore we add another underscore to it
            var id = new StringBuilder(name.Length + 1);

            // split folder name into subnames separated by '.', if any
            var subNames = name.Split(new char[] { '.' });

            // convert each subname separately
            id.Append(MakeValidSubFolderIdentifier(subNames[0]));

            for (int i = 1; i < subNames.Length; i++)
            {
                id.Append('.');
                id.Append(MakeValidSubFolderIdentifier(subNames[i]));
            }

            // folder name cannot be a single underscore - add another underscore to it
            if (id.ToString() == "_")
            {
                id.Append('_');
            }

            return id.ToString();
        }

        /// <summary>
        /// Make a folder subname into identifier
        /// </summary>
        private static string MakeValidSubFolderIdentifier(string subName)
        {
            if (subName.Length == 0)
            {
                return subName;
            }

            // give string length to avoid reallocations; +1 since the resulting string may be one char longer than the
            // original - if the first character is an invalid first identifier character but a valid subsequent one,
            // we prepend an underscore to it.
            var id = new StringBuilder(subName.Length + 1);

            // the first character has stronger restrictions than the rest
            if (!IsValidIdFirstChar(subName[0]))
            {
                // if the first character is not even a valid subsequent character, replace it with an underscore
                if (!IsValidIdChar(subName[0]))
                {
                    id.Append('_');
                }
                // if it is a valid subsequent character, prepend an underscore to it
                else
                {
                    id.Append('_');
                    id.Append(subName[0]);
                }
            }
            else
            {
                id.Append(subName[0]);
            }

            // process the rest of the subname
            for (int i = 1; i < subName.Length; i++)
            {
                if (!IsValidIdChar(subName[i]))
                {
                    id.Append('_');
                }
                else
                {
                    id.Append(subName[i]);
                }
            }

            return id.ToString();
        }

        /// <summary>
        /// Is the character a valid first identifier character?
        /// </summary>
        private static bool IsValidIdFirstChar(char c)
        {
            return
                char.IsLetter(c) ||
                CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.ConnectorPunctuation;
        }

        /// <summary>
        /// Is the character a valid identifier character?
        /// </summary>
        private static bool IsValidIdChar(char c)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);

            return
                char.IsLetterOrDigit(c) ||
                cat == UnicodeCategory.ConnectorPunctuation ||
                cat == UnicodeCategory.NonSpacingMark ||
                cat == UnicodeCategory.SpacingCombiningMark ||
                cat == UnicodeCategory.EnclosingMark;
        }

        public static string EnsureResourceExtension(string logicalName, string resourceFilePath)
        {
            string resourceExtension = Path.GetExtension(resourceFilePath);
            if (!string.IsNullOrEmpty(resourceExtension))
            {
                if (!logicalName.EndsWith(resourceExtension, StringComparison.Ordinal))
                {
                    logicalName += resourceExtension;
                }
            }

            return logicalName;
        }
    }
}
