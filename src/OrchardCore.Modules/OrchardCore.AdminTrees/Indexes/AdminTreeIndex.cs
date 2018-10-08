using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.AdminTrees.Models;
using YesSql.Indexes;

namespace OrchardCore.AdminTrees.Indexes
{
    public class AdminTreeIndex : MapIndex
    {
        public string Name { get; set; }
    }

    public class AdminTreeIndexProvider: IndexProvider<AdminTree>
    {
        public override void Describe(DescribeContext<AdminTree> context)
        {
            context.For<AdminTreeIndex>()
                .Map(adminTree =>
                {
                    return new AdminTreeIndex
                    {
                        Name = adminTree.Name
                    };
                });
        }
    }
}
