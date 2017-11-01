using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OrchardCore.GraphQL.Client
{
    public class ContentPartBuilder
    {
        private string _contentPartName;

        private Dictionary<string, string> _keysWithValues = new Dictionary<string, string>();
        private List<string> _keys = new List<string>();

        public ContentPartBuilder(string contentPartName)
        {
            _contentPartName = contentPartName;
        }

        public ContentPartBuilder AddField(string name, string value)
        {
            _keysWithValues.Add(name, value);

            return this;
        }

        public ContentPartBuilder AddField(string name)
        {
            _keys.Add(name);

            return this;
        }

        public virtual string Build() { return null; }
    }
}
