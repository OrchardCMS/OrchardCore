using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Media.Processing
{
    public class CacheEntry
    {
        public string FileKey { get; }

        public string[] FilePaths { get; }

        public CacheEntry(string fileKey, string[] filePaths)
        {
            FileKey = fileKey;
            FilePaths = filePaths;
        }
    }
}
