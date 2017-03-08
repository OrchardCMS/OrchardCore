using System.Dynamic;

namespace Orchard.Tokens.Services
{
    public class NullTokenizer : ITokenizer
    {
        public dynamic CreateViewModel()
        {
            return new ExpandoObject();
        }

        public string Tokenize(string template, object context)
        {
            return template;
        }
    }
}
