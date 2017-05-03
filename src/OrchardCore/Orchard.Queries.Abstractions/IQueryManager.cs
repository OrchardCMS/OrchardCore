using System.Collections.Generic;

namespace Orchard.Queries
{
    public interface IQueryManager
    {
        IEnumerable<IQuerySource> GetQuerySources();
    }
}
