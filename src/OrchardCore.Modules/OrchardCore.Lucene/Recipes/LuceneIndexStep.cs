using System;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Lucene.Recipes
{
    /// <summary>
    /// This recipe step creates a lucene index.
    /// </summary>
    public class LuceneIndexStep : IRecipeStepHandler
    {
        private readonly LuceneIndexManager _luceneIndexManager;

        public LuceneIndexStep(LuceneIndexManager luceneIndexManager)
        {
            _luceneIndexManager = luceneIndexManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "lucene-index", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<LuceneIndexModel>();

            foreach(var index in model.Indices)
            {
                if (!_luceneIndexManager.Exists(index))
                {
                    await _luceneIndexManager.CreateIndex(index);
                }
            }            
        }

        private class LuceneIndexModel
        {
            public string[] Indices { get; set; }
        }
    }
}
