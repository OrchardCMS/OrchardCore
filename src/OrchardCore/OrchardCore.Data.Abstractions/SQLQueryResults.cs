using System;
using System.Collections.Generic;
using OrchardCore.Queries;

namespace OrchardCore.Data
{
    [Obsolete("This class has been deprecated and will be removed on the upcoming major release.", error: true)]
    public class SQLQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
    }
}
