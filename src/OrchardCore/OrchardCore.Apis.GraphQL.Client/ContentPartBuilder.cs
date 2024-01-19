using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentPartBuilder
    {
        private readonly string _contentPartName;
        private readonly Dictionary<string, string> _keysWithValues = new();
        private readonly List<string> _keys = new();

        public ContentPartBuilder(string contentPartName)
        {
            _contentPartName = contentPartName;
        }

        public ContentPartBuilder AddField(string name, string value)
        {
            _keysWithValues.Add(name.ToGraphQLStringFormat(), value);

            return this;
        }

        public ContentPartBuilder AddField(string name)
        {
            _keys.Add(name);

            return this;
        }

        internal string Build()
        {
            var sb = new StringBuilder();
            sb.Append(_contentPartName).Append(": {{ ");

            for (var i = 0; i < _keysWithValues.Count; i++)
            {
                var item = _keysWithValues.ElementAt(i);
                sb.Append(item.Key).Append(": \"").Append(item.Value).Append('"');

                if (i < (_keysWithValues.Count - 1))
                {
                    sb.Append(' ');
                }
            }

            foreach (var item in _keys)
            {
                sb.Append(item).Append(' ');
            }

            sb.Append(" }");

            return sb.ToString();
        }
    }
}
