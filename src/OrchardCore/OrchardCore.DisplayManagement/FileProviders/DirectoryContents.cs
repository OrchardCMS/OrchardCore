using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.DisplayManagement.FileProviders
{
    public class DirectoryContents : IDirectoryContents
    {
        private readonly IEnumerable<IFileInfo> _entries;

        public DirectoryContents(IEnumerable<IFileInfo> entries)
        {
            _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public bool Exists
        {
            get { return true; }
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entries.GetEnumerator();
        }
    }
}
