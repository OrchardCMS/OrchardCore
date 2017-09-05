using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.DisplayManagement.FileProviders
{
    public class ContentFileInfo : IFileInfo
    {
        private readonly byte[] _content;
        private readonly string _name;

        public ContentFileInfo(string name, string content)
        {
            _name = name;
            _content = Encoding.UTF8.GetBytes(content);
        }

        public bool Exists => true;

        public long Length
        {
            get { return _content.Length; }
        }

        public string PhysicalPath => null;

        public string Name
        {
            get { return _name; }
        }

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
