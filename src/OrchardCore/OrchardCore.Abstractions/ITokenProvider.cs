namespace OrchardCore;

public interface ITokenProvider
{
    Task<TokenResult> GetTokenAsync();
}
