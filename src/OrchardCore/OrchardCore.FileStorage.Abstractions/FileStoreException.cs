using System;

namespace OrchardCore.FileStorage
{
    public class FileStoreException : Exception
    {
        public FileStoreException(string message) : base(message)
        {
        }

        public FileStoreException(string message, string fileName) : this(message)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException($"'{nameof(fileName)}' cannot be null or empty", nameof(fileName));
            }

            FileName = fileName;
        }

        public FileStoreException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string FileName { get;  }
    }
}
