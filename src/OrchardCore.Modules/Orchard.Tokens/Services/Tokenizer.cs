using System;
using Orchard.Tokens.Handlebars;

namespace Orchard.Tokens.Services
{
    public class Tokenizer : ITokenizer
    {
        private readonly HandlebarsTokenizer _tokenizer;
        private readonly IServiceProvider _serviceProvider;

        public Tokenizer(HandlebarsTokenizer tokenizer, IServiceProvider serviceProvider)
        {
            _tokenizer = tokenizer;
            _serviceProvider = serviceProvider;
        }
        
        public string Tokenize(string template, dynamic context)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }

            return _tokenizer.Tokenize(template, context, _serviceProvider);
        }

        public dynamic CreateViewModel()
        {
            return new Composite();
        }
    }
}
