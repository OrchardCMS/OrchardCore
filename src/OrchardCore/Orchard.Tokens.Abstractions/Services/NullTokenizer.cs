using System.Collections.Generic;

namespace Orchard.Tokens.Services
{
    public class NullTokenizer : ITokenizer
    {
        public string Tokenize(string template, IDictionary<string, object> context)
        {
            return template;
        }
    }
}
