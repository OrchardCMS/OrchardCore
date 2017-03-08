using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orchard.Deployment
{
    /// <summary>
    /// The state of a deployment plan built by sources.
    /// </summary>
    public class DeploymentPlanResult
    {
        public DeploymentPlanResult(IFileBuilder fileBuilder)
        {
            FileBuilder = fileBuilder;

            Recipe = new JObject();
            Recipe["name"] = "";
            Recipe["displayName"] = "";
            Recipe["description"] = "";
            Recipe["author"] = "";
            Recipe["website"] = "";
            Recipe["version"] = "";
            Recipe["issetuprecipe"] = false;
            Recipe["categories"] = "";
            Recipe["tags"] = new JArray();
        }

        public JObject Recipe { get; }
        public IList<JObject> Steps { get; } = new List<JObject>();
        public IFileBuilder FileBuilder { get; }
        public async Task FinalizeAsync()
        {
            Recipe["steps"] = new JArray(Steps);

            // Add the recipe steps as its own file content
            await FileBuilder.SetFileAsync("Recipe.json", Encoding.UTF8.GetBytes(Recipe.ToString(Formatting.Indented)));

        }
    }
}
