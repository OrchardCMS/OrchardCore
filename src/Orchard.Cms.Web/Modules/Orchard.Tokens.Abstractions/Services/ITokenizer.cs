namespace Orchard.Tokens.Services
{
    public interface ITokenizer
    {
        string Tokenize(string text, object data);
    }
}
