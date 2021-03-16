using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.DisplayManagement.FileProviders
{
    public class ContentFileInfo : IFileInfo
    {
        private readonly byte[] _content;

        public ContentFileInfo(string name, string content)
        {
            Name = name;
            _content = Encoding.UTF8.GetBytes(content);
        }

        public bool Exists => true;

        public long Length
        {
            get { return _content.Length; }
        }

        public string PhysicalPath => null;

        public string Name { get; }

        public DateTimeOffset LastModified
        {
            get { return DateTimeOffset.MinValue; }
        }

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            return new MemoryStream(_content);
        }
    }
}
