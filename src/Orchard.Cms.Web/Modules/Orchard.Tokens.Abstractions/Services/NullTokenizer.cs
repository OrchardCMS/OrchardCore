namespace Orchard.Tokens.Services
{
    public class NullTokenizer : ITokenizer
    {
        public string Tokenize(string text, object data)
        {
            return text;
        }
    }
}
