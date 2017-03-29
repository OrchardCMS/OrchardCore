using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using HandlebarsDotNet;

namespace Orchard.Tokens.Handlebars
{
    public class HandlebarsTokenizer
    {
        private static readonly Regex _tokens = new Regex(@"(\{\{\{[^\{\}]*\}\}\})|(\{\{[^\{\}]*\}\})", RegexOptions.Compiled);

        private readonly ConcurrentDictionary<string, Func<object, string>> _renderers;
        private readonly IHandlebars _handlebars;

        public HandlebarsTokenizer()
        {
            _handlebars = HandlebarsDotNet.Handlebars.Create();
            _renderers = new ConcurrentDictionary<string, Func<object, string>>();
        }

        public IHandlebars Handlebar
        {
            get
            {
                return _handlebars;
            }
        }

        public string Tokenize(string template, dynamic context, IServiceProvider serviceProvider)
        {
            context.ServiceProvider = serviceProvider;

            return _tokens.Replace(template, match =>
            {
                var render = _renderers.GetOrAdd(match.Value, t => _handlebars.Compile(t));
                return render.Invoke(context);
            });
        }
    }
}
