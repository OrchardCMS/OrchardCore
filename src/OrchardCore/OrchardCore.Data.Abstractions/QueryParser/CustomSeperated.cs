using Parlot;
using Parlot.Fluent;
using System;
using System.Collections.Generic;

namespace OrchardCore.Data.QueryParser
{
    public class CustomSeparated<U, T> : Parser<List<T>>
    {
        private readonly Parser<U> _separator;
        private readonly Parser<T> _parser;

        public CustomSeparated(Parser<U> separator, Parser<T> parser)
        {
            _separator = separator ?? throw new ArgumentNullException(nameof(separator));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));

            // TODO: more optimization could be done for other literals by creating different implementations of this class instead of doing 
            // ifs in the Parse method. Then the builders could check the kind of literal used and return the correct implementation.
        }

        public override bool Parse(ParseContext context, ref ParseResult<List<T>> result)
        {
            context.EnterParser(this);

            List<T> results = null;

            var start = 0;
            var end = 0;

            var first = true;
            var parsed = new ParseResult<T>();
            var separatorResult = new ParseResult<U>();

            while (true)
            {
                if (!_parser.Parse(context, ref parsed))
                {
                    if (!first)
                    {
                        break;
                    }

                    return false;
                }

                if (first)
                {
                    start = parsed.Start;
                }

                end = parsed.End;
                results ??= new List<T>();
                results.Add(parsed.Value);

                var position = context.Scanner.Cursor.Position;

                if (_separator.Parse(context, ref separatorResult))
                {
                    context.Scanner.Cursor.ResetPosition(position);
                }
                else
                {
                    break;
                }
            }

            result = new ParseResult<List<T>>(start, end, results);
            return true;
        }
    }
}
