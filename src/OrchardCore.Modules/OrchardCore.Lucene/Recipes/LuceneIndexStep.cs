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

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "lucene-index", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var model = context.Step.ToObject<LuceneIndexModel>();

            foreach(var index in model.Indices)
            {
                if (!_luceneIndexManager.Exists(index))
                {
                    _luceneIndexManager.CreateIndex(index);
                }
            }

            return Task.CompletedTask;  
        }
    }

    public class LuceneIndexModel
    {
        public string[] Indices { get; set; }
    }
}