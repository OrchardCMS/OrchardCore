using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.FileSystem.AppData;
using Orchard.Recipes.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeStepQueue : IRecipeStepQueue
    {
        private readonly string _recipeQueueFolder = "RecipeQueue" + Path.DirectorySeparatorChar;

        private readonly IAppDataFolder _appDataFolder;
        private readonly ILogger _logger;

        public RecipeStepQueue(IAppDataFolder appDataFolder,
            ILogger<RecipeStepQueue> logger)
        {
            _appDataFolder = appDataFolder;
            _logger = logger;
        }

        public async Task<RecipeStepDescriptor> DequeueAsync(string executionId)
        {
            _logger.LogInformation("Dequeuing recipe steps.");
            var executionFolderPath = _appDataFolder.Combine(_recipeQueueFolder, executionId);
            if (!_appDataFolder.DirectoryExists(executionFolderPath))
            {
                return null;
            }

            RecipeStepDescriptor recipeStep = null;
            int stepIndex = GetFirstStepIndex(executionId);

            if (stepIndex >= 0)
            {
                var stepPath = _appDataFolder.Combine(_recipeQueueFolder, executionId, stepIndex.ToString());
                
                var stepElement = JObject.Parse(await _appDataFolder.ReadFileAsync(stepPath));
                var stepName = stepElement.Value<string>("name");
                var recipeName = stepElement.Value<string>("recipename");
                var stepId = stepElement.Value<string>("id");

                _logger.LogInformation("Dequeuing recipe step '{0}'.", stepName);
                recipeStep = new RecipeStepDescriptor
                {
                    Id = stepId,
                    RecipeName = recipeName,
                    Name = stepName,
                    Step = stepElement["step"]
                };
                _appDataFolder.DeleteFile(stepPath);
            }

            if (stepIndex < 0)
            {
                _appDataFolder.DeleteFile(executionFolderPath);
            }

            return recipeStep;
        }

        public async Task EnqueueAsync(string executionId, RecipeStepDescriptor recipeStep)
        {
            _logger.LogInformation("Enqueuing recipe step '{0}'.", recipeStep.Name);
            var recipeStepElement = new JObject();
            recipeStepElement.Add(new JProperty("id", recipeStep.Id));
            recipeStepElement.Add(new JProperty("recipename", recipeStep.RecipeName));
            recipeStepElement.Add(new JProperty("name", recipeStep.Name));
            recipeStepElement.Add(new JProperty("step", recipeStep.Step));

            if (_appDataFolder.DirectoryExists(_appDataFolder.Combine(_recipeQueueFolder, executionId)))
            {
                int stepIndex = GetLastStepIndex(executionId) + 1;
                await _appDataFolder.CreateFileAsync(
                    _appDataFolder.Combine(_recipeQueueFolder, executionId, stepIndex.ToString()),
                    recipeStepElement.ToString());
            }
            else
            {
                await _appDataFolder.CreateFileAsync(
                    _appDataFolder.Combine(_recipeQueueFolder, executionId, "0"),
                    recipeStepElement.ToString());
            }
        }

        private int GetFirstStepIndex(string executionId)
        {
            var stepFiles = _appDataFolder.ListFiles(Path.Combine(_recipeQueueFolder, executionId));
            if (!stepFiles.Any())
            {
                return -1;
            }
            var currentSteps = stepFiles.Select(stepFile => int.Parse(stepFile.Name.Substring(stepFile.Name.LastIndexOf('/') + 1))).ToList();
            currentSteps.Sort();
            return currentSteps[0];
        }

        private int GetLastStepIndex(string executionId)
        {
            int lastIndex = -1;
            var stepFiles = _appDataFolder.ListFiles(Path.Combine(_recipeQueueFolder, executionId));
            // we always have only a handful of steps.
            foreach (var stepFile in stepFiles)
            {
                int stepOrder = int.Parse(stepFile.Name.Substring(stepFile.Name.LastIndexOf('/') + 1));
                if (stepOrder > lastIndex)
                    lastIndex = stepOrder;
            }

            return lastIndex;
        }
    }
}
