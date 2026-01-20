namespace OrchardCore.Email;

public class InvalidEmailProviderException : ArgumentOutOfRangeException
{
    public InvalidEmailProviderException(string name)
        : base(nameof(name), $"'{name}' is an invalid Email provider name.")
    {
    }
}
