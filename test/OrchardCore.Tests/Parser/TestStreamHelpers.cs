using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Tests.Parser
{
    public static class TestStreamHelpers
    {
        public static readonly string ArbitraryFilePath = "Unit tests do not touch file system";

        public static IFileProvider StringToFileProvider(string str)
        {
            return new TestFileProvider(str);
        }

        private class TestFile : IFileInfo
        {
            private readonly string _str;

            public TestFile(string str)
            {
                _str = str;
            }

            public Stream CreateReadStream()
            {
                return StringToStream(_str);
            }

            public bool Exists { get; } = true;

            public long Length
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string PhysicalPath
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string Name
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public DateTimeOffset LastModified
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool IsDirectory { get; } = false;
        }

        private class TestFileProvider : IFileProvider
        {
            private readonly string _str;

            public TestFileProvider(string str)
            {
                _str = str;
            }

            public IFileInfo GetFileInfo(string subpath)
            {
                return new TestFile(_str);
            }

            public IDirectoryContents GetDirectoryContents(string subpath)
            {
                throw new NotImplementedException();
            }

            public IChangeToken Watch(string filter)
            {
                throw new NotImplementedException();
            }
        }

        public static Stream StringToStream(string str)
        {
            var memStream = new MemoryStream();
            var textWriter = new StreamWriter(memStream);
            textWriter.Write(str);
            textWriter.Flush();
            memStream.Seek(0, SeekOrigin.Begin);

            return memStream;
        }

        public static string StreamToString(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
