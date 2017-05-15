using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Orchard.Tokens.Services
{
    public interface ITokenizer
    {
        /// <summary>
        /// Evaluates the template with the specified data object as the context.
        /// The method requires an <see cref="IDictionary{TKey, TValue}"/> for performance reasons
        /// as reflection over dynamic and <see cref="JObject"/> is very expensive.
        /// </summary>
        string Tokenize(string template, IDictionary<string, object> context);
    }

    public static class TokenizerExtensions
    {
        public static string Tokenize(this ITokenizer tokenizer, string template, JObject obj)
        {
            var parameters = obj.ToDictionary<KeyValuePair<string, JToken>, string, object>(x => x.Key, y =>
            {
                switch (y.Value.Type)
                {
                    case JTokenType.Boolean:
                        return y.Value.ToObject<bool>();
                    case JTokenType.String:
                        return y.Value.ToObject<string>();
                    case JTokenType.Float:
                        return y.Value.ToObject<float>();
                }

                return y.Value.ToString();
            });

            return tokenizer.Tokenize(template, parameters);
        }
    }
}
