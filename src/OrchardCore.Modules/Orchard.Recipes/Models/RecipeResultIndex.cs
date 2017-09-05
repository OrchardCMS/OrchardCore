using System.Linq;
using YesSql.Indexes;

namespace OrchardCore.Recipes.Models
{
    public class RecipeResultIndex : MapIndex
    {
        public string ExecutionId { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }
    }

    public class RecipeResultIndexProvider : IndexProvider<RecipeResult>
    {
        public override void Describe(DescribeContext<RecipeResult> context)
        {
            context
                .For<RecipeResultIndex>()
                .Map(result => new RecipeResultIndex
                {
                    ExecutionId = result.ExecutionId,
                    IsCompleted = result.IsCompleted,
                    TotalSteps = result.Steps.Count,
                    CompletedSteps = result.Steps.Count(x => x.IsCompleted)
                });
        }
    }
}
