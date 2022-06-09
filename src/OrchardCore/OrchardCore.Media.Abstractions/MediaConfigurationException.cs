using System;

namespace OrchardCore.Media;

public class MediaConfigurationException : Exception
{
    public MediaConfigurationException(string message) : base(message)
    {
    }

    public MediaConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
