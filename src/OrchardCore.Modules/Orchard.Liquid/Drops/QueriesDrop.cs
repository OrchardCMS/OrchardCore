using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Newtonsoft.Json.Linq;
using Orchard.Queries;

namespace Orchard.Liquid.Drops
{
    public class QueriesDrop : Drop, IIndexable
    {
        private readonly IQueryManager _queryManager;

        public QueriesDrop(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }
        
        public override object BeforeMethod(string method)
        {
            var query = _queryManager.GetQueryAsync(method.ToString()).GetAwaiter().GetResult();

            if (query == null)
            {
                return null;
            }

            var result = _queryManager.ExecuteQueryAsync(query, new Dictionary<string, object>()).GetAwaiter().GetResult();

            if (result is IEnumerable<JObject>)
            {
                // BUG: Somehow Liquid doesn't accept JObject collections directly unlike ContentItem.
                return ((IEnumerable<JObject>)result).Select(x => new JTokenDrop(x)).ToArray();
            }

            return result;
        }

        bool IIndexable.ContainsKey(object key)
        {
            return true;
        }
    }
}
