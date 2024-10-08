namespace OrchardCore;

public interface IClientIPAddressAccessor
{
    Task<string> GetIPAddressAsync();
}
