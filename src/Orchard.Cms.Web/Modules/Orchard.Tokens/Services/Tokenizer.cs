using System;
using System.Collections.Concurrent;
using System.IO;
using HandlebarsDotNet;
using Orchard.ContentManagement;

namespace Orchard.Tokens.Services
{
    public class Tokenizer : ITokenizer
    {
        private readonly ConcurrentDictionary<string, Func<object, string>> _renderers;
        private readonly IHandlebars _handlebars;

        public Tokenizer()
        {
            _handlebars = Handlebars.Create();
            _renderers = new ConcurrentDictionary<string, Func<object, string>>();

            _handlebars.RegisterHelper("dateformat", (output, context, arguments) =>
            {
                var format = arguments[0].ToString();
                output.Write(DateTime.Now.ToString(format));
            });

            _handlebars.RegisterHelper("slug", (output, context, arguments) =>
            {
                ContentItem contentItem = context.content;
                string title = contentItem.Content.TitlePart.Title;
                var slug = title?.ToLower().Replace(" ", "-");
                output.Write(slug);
            });
        }

        public string Tokenize(string text, object data)
        {
            // If the text doesn't have handlebars blocks to process, return it
            if (!text.Contains("{{") || !text.Contains("}}"))
            {
                return text;
            }

            // TODO: extract all the handlebars blocks and build the resulting text by concatenating 
            // the fragments.

            var render = _renderers.GetOrAdd(text, template => _handlebars.Compile(template));

            return render(data);
        }
    }
}
