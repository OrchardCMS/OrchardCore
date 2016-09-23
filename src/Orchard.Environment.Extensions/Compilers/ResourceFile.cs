using System;
using System.IO;

namespace Orchard.Environment.Extensions.Compilers
{
    internal struct ResourceFile
    {
        public ResourceFile(FileInfo file, ResourceFileType type)
        {
            File = file;
            Type = type;
        }

        public FileInfo File { get; }

        public ResourceFileType Type { get; }

        public static ResourceFile Create(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            ResourceFileType type;
            switch (fileInfo.Extension.ToLowerInvariant())
            {
                case ".resx":
                    type = ResourceFileType.Resx;
                    break;
                case ".resources":
                    type = ResourceFileType.Resources;
                    break;
                case ".dll":
                    type = ResourceFileType.Dll;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported resource file");
            }

            return new ResourceFile(fileInfo, type);
        }
    }
}
