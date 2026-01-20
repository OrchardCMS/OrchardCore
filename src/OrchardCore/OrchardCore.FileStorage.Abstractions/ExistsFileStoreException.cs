namespace OrchardCore.FileStorage;

public class ExistsFileStoreException : FileStoreException
{
    public ExistsFileStoreException(string message) : base(message)
    {
    }

    public ExistsFileStoreException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
