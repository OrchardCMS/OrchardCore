using Microsoft.Extensions.Localization;

namespace OrchardCore.FileStorage;

public class FileStoreException : Exception
{
    public LocalizedString UserMessage { get; }

    public FileStoreException(string message, LocalizedString userMessage) : this(message)
    {
        UserMessage = userMessage;
    }

    public FileStoreException(string message) : base(message)
    {
    }

    public FileStoreException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
