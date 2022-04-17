using Microsoft.Extensions.Primitives;

namespace OrchardCore.Security
{
    public class ContentTypeOptionsValue
    {
        private readonly string _option;

        internal ContentTypeOptionsValue(string option) => _option = option;

        public static readonly ContentTypeOptionsValue NoSniff = new("nosniff");

        public static implicit operator StringValues(ContentTypeOptionsValue option) => option.ToString();

        public static implicit operator string(ContentTypeOptionsValue option) => option.ToString();

        public override string ToString() => _option;
    }
}
