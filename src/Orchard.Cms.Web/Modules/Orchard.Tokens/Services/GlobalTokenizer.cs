using System;
using System.Collections.Concurrent;
using HandlebarsDotNet;

namespace Orchard.Tokens.Services
{
    public class HandlebarsTokenizer
    {
        private readonly ConcurrentDictionary<string, Func<object, string>> _renderers;
        private readonly IHandlebars _handlebars;

        public HandlebarsTokenizer()
        {
            _handlebars = Handlebars.Create();
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
            // If the text doesn't have handlebars blocks to process, return it
            if (!template.Contains("{{") || !template.Contains("}}"))
            {
                return template;
            }

            // TODO: extract all the handlebars blocks and build the resulting text by concatenating 
            // the fragments.

            context.ServiceProvider = serviceProvider;
            var render = _renderers.GetOrAdd(template, t => _handlebars.Compile(t));

            return render(context);
        }
    }
}
