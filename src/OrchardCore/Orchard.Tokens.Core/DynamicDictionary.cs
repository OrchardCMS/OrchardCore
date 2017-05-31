using System.Collections.Generic;
using System.Dynamic;

namespace Orchard.Tokens
{
    public class DynamicDictionary : DynamicObject
    {
        private readonly IDictionary<string, object> _dictionary;

        public DynamicDictionary(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public DynamicDictionary() : this(new Dictionary<string, object>())
        {
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dictionary[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _dictionary.TryGetValue(binder.Name, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dictionary.Keys;
        }
    }
}
