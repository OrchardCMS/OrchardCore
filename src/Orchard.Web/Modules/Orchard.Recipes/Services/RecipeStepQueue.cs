//using Microsoft.Extensions.Logging;
//using Orchard.Recipes.Models;
//using System.Linq;
//using System.Threading.Tasks;
//using YesSql.Core.Services;

//namespace Orchard.Recipes.Services
//{
//    public class RecipeStepQueue : IRecipeStepQueue
//    {
//        private readonly ISession _session;
//        private readonly ILogger _logger;

//        public RecipeStepQueue(ISession session,
//            ILogger<RecipeStepQueue> logger)
//        {
//            _session = session;
//            _logger = logger;
//        }

//        public async Task<RecipeStepDescriptor> DequeueAsync(string executionId)
//        {
//        }

//        public async Task EnqueueAsync(string executionId, RecipeStepDescriptor recipeStep)
//        {
//            _logger.LogInformation("Enqueuing recipe step '{0}'.", recipeStep.Name);
            
//            _session.Save(new RecipeStepResult
//            {
//                ExecutionId = executionId,
//                StepId = recipeStep.Id,
//                RecipeName = recipeStep.RecipeName,
//                StepName = recipeStep.Name
//            });

//            await _session.CommitAsync();

//            _logger.LogInformation("Enqueued recipe step '{0}'.", recipeStep.Name);
//        }
//    }

//    public class RecipeStepExecutionDescriptor
//    {
//        public string ExecutionId { get; set; }
//        public string StepId { get; set; }
//        public RecipeStepDescriptor Step { get; set; }
//    }
//}
