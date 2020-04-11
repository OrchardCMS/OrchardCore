using System.Collections.Generic;
using System.Text.Encodings.Web;

namespace OrchardCore.DisplayManagement.Notify
{
    internal class NotifyEntryComparer : IEqualityComparer<NotifyEntry>
    {
        private readonly HtmlEncoder _htmlEncoder;

        public NotifyEntryComparer(HtmlEncoder htmlEncoder)
        {
            _htmlEncoder = htmlEncoder;
        }

        public bool Equals(NotifyEntry x, NotifyEntry y)
        {
            return x.Type == y.Type && x.GetMessageAsString(_htmlEncoder) == y.GetMessageAsString(_htmlEncoder);
        }

        public int GetHashCode(NotifyEntry obj)
        {
            return obj.GetMessageAsString(_htmlEncoder).GetHashCode() & 23 * obj.Type.GetHashCode();
        }
    }
}
