using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Extensions.Caching.Memory;

namespace Orchard.Tokens.Services
{
    public class Tokenizer : ITokenizer
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TokensHelper _tokenHelper;

        public Tokenizer(IMemoryCache memoryCache, TokensHelper tokenHelper)
        {
            _memoryCache = memoryCache;
            _tokenHelper = tokenHelper;
        }
        
        public string Tokenize(string template, IDictionary<string, object> context)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }

            var render = _memoryCache.GetOrCreate(template, t => _tokenHelper.Handlebars.Compile(template));
            var result = render.Invoke(new DynamicDictionary(context));
            return result;
        }
    }
}
