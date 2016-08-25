using System;
using YesSql.Core.Indexes;

namespace Orchard.Recipes.Models
{
    public class RecipeResultIndex : MapIndex
    {
        public string ExecutionId { get; set; }
    }

    public class RecipeResultIndexProvider : IndexProvider<RecipeResult>
    {
        public override void Describe(DescribeContext<RecipeResult> context)
        {
            context
                .For<RecipeResultIndex>()
                .Map(result => new RecipeResultIndex
                {
                    ExecutionId = result.ExecutionId
                });
        }
    }
}
