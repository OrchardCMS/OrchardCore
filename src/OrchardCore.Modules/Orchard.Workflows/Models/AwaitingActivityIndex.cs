using System;
using System.Collections.Generic;
using System.Text;
using YesSql.Indexes;

namespace Orchard.Workflows.Models
{
    public class AwaitingActivityIndex : MapIndex
    {
    }

    public class AwaitingActivityndexProvider : IndexProvider<AwaitingActivity>
    {
        public override void Describe(DescribeContext<AwaitingActivity> context)
        {
            context.For<AwaitingActivityIndex>()
                .Map(awaitingActivity =>
                {
                    return new AwaitingActivityIndex
                    {

                    };
                });
        }
    }
}
