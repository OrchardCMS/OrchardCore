using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Orchard.Tests.Configuration
{
    public class RecipeParserTests : IDisposable
    {
        private string _tempFolderName;

        private IFileInfo _fileInfo;

        public RecipeParserTests()
        {
            _tempFolderName = Path.GetTempFileName();

            var json = @"{" +
"  \"name\": \"core\"," +
"  \"description\": \"core descriptor.\"," +
"  \"author\": \"The Orchard Team\"," +
"  \"website\": \"http://orchardproject.net\"," +
"  \"version\": \"2.0\"," +
"  \"issetuprecipe\": true," +
"  \"categories\": [ \"default\" ]," +
"  \"tags\": [ \"developer\" ]," +
"  \"steps\": [" +
"    {" +
"      \"name\": \"feature\"," +
"      \"disable\": [ ]," +
"      \"enable\": [" +
"        \"Orchard.Logging.Console\"," +
"        \"Orchard.Hosting\"" +
"      ]" +
"    }," +
"    {" +
"      \"name\": \"feature2\"," +
"      \"disable\": [ ]," +
"      \"enable\": [" +
"        \"Orchard.Logging.Console\"," +
"        \"Orchard.Hosting\"" +
"      ]" +
"    }" +
"  ]" +
"}";
            File.WriteAllText(_tempFolderName, json);

            _fileInfo = new PhysicalFileInfo(new FileInfo(_tempFolderName));
        }

        public void Dispose()
        {
            File.Delete(_tempFolderName);
        }

        [Fact]
        public void ShouldParseRecipeDescriptor()
        {
            var parser = new JsonRecipeParser();
            var descriptor = parser.ParseRecipe(_fileInfo);
            Assert.Equal("core", descriptor.Name);
            Assert.Equal("core descriptor.", descriptor.Description);
            Assert.Equal("The Orchard Team", descriptor.Author);
            Assert.Equal("http://orchardproject.net", descriptor.WebSite);
            Assert.Equal("2.0", descriptor.Version);
            Assert.Equal(true, descriptor.IsSetupRecipe);
            Assert.Equal("default", descriptor.Categories[0]);
            Assert.Equal("developer", descriptor.Tags[0]);
        }

        [Fact]
        public void ProcessingRecipeShouldYieldUniqueIdsForSteps()
        {
            var recipeParser = new JsonRecipeParser();

            List<RecipeStepDescriptor> recipeSteps = new List<RecipeStepDescriptor>();
            recipeParser.ProcessRecipe(_fileInfo, (descripor, stepDescriptor) => {
                recipeSteps.Add(stepDescriptor);
            });

            // Assert that each step has a unique ID.
            Assert.True(recipeSteps.GroupBy(x => x.Id).All(y => y.Count() == 1));
        }


    }
}