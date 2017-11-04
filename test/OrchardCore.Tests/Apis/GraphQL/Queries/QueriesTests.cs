using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Tests.Apis.GraphQL.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Queries
{
    public class QueriesTests : IClassFixture<SiteContext>
    {
        private SiteContext _context;

        public QueriesTests(SiteContext context)
        {
            _context = context;
        }


    }
}
