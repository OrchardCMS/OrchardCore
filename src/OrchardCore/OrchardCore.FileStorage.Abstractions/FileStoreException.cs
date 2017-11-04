using System;

namespace OrchardCore.FileStorage
{
    public class FileStoreException : Exception
    {
        public FileStoreException(string message) : base(message)
        {
        }
    }
}
